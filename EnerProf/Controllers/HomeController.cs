using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using EnerProf.DataBaseClasses;
using EnerProf.Models;
namespace EnerProf.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            Database.SetInitializer(new DropCreateDatabaseAlways<CodeContext>());

            using (var context = new CodeContext())
            {
                Company company = new Company { Name = "Enerpac" };
                Category cat1 = new Category { Img = "/images/cat1.png", Name = "Цилиндры", IsOnStartPage = true, Parent = null };
                Category cat2 = new Category { Name = "Алюминиевые", Img = "/images/gidraulic.jpg", IsOnStartPage = false, Parent = cat1 };
                context.Categories.Add(cat2);
                Product product1 = new Product { Name = "RC-цилиндр", Img = "/images/gidraulic.jpg", Description = "", Category = cat2, Company = company };
                Product product2 = new Product { Name = "RB-цилиндр", Img = "/images/gidraulic.jpg", Description = "", Category = cat2, Company = company };

                Model model1 = new Model { Name = "RC-1000", Product = product1 };
                Model model2 = new Model { Name = "RC-2000", Product = product1 };
                Model model3 = new Model { Name = "RB-1000", Product = product2 };
                Model model4 = new Model { Name = "RB-2000", Product = product2 };
                //context.Products.Add(product1);
                Unit unit1 = new Unit { Name = "мм" };

                AttributeModel attr1 = new AttributeModel() { Category = cat1, Name = "Ширина", Status = true, Type = Types.Double, Unit = unit1 };
                AttributeModel attr2 = new AttributeModel() { Category = cat2, Name = "Радиус", Status = true, Type = Types.Double, Unit = unit1 };
                AttributeModel attr3 = new AttributeModel() { Category = cat2, Name = "Производитель", Status = true, Type = Types.String, };

                context.AttributesValues.Add(new AttributesValues { Attribute = attr1, Model = model1, Double_Value = 20 });
                context.AttributesValues.Add(new AttributesValues { Attribute = attr1, Model = model2, Double_Value = 30 });
                context.AttributesValues.Add(new AttributesValues { Attribute = attr1, Model = model3, Double_Value = 40 });
                context.AttributesValues.Add(new AttributesValues { Attribute = attr1, Model = model4, Double_Value = 50 });

                context.AttributesValues.Add(new AttributesValues { Attribute = attr2, Model = model1, Double_Value = 50 });
                context.AttributesValues.Add(new AttributesValues { Attribute = attr2, Model = model2, Double_Value = 100 });
                context.AttributesValues.Add(new AttributesValues { Attribute = attr2, Model = model3, Double_Value = 70 });
                context.AttributesValues.Add(new AttributesValues { Attribute = attr2, Model = model4, Double_Value = 120 });

                context.AttributesValues.Add(new AttributesValues { Attribute = attr3, Model = model1, String_Value = "Enerpac" });
                context.AttributesValues.Add(new AttributesValues { Attribute = attr3, Model = model2, String_Value = "Enerpac" });
                context.AttributesValues.Add(new AttributesValues { Attribute = attr3, Model = model3, String_Value = "WallMag" });
                context.AttributesValues.Add(new AttributesValues { Attribute = attr3, Model = model4, String_Value = "WallMag" });


                context.SaveChanges();

                IEnumerable<Category> model_categories = context.Categories.ToList();

                foreach (var category in model_categories)
                    category.Sub = model_categories.Where(cat => cat.Parent == category).ToList();

                ViewBag.ListOfCategories = model_categories;
                ViewBag.ListOfIndustries = context.Industries.ToList();
                ViewBag.Clients = context.Clients;

                ViewBag.Title = "Enerprof";
            }

            return View();
        }

        public ActionResult categories(int? parent)
        {
            Category model;
            List<Category> cats = new List<Category>();
            using (var context = new CodeContext())
            {
                if (context.Categories.Where(cat => cat.Parent.Id == parent).ToList().Count == 0)
                    model = new Category();
                else
                    model = context.Categories.Where(cat => cat.Id == parent).First();
                if (parent != null)
                {
                    ViewBag.CatName = context.Categories.Where(cat => cat.Id == parent).Select(name => name.Name).First();
                    model.Description = context.Categories.Where(cat => cat.Id == parent).Select(desc => desc.Description).First();
                    cats = context.Categories.Where(cat => cat.Parent.Id == parent).ToList();
                }
                else
                {
                    cats = context.Categories.Where(parent_cat => parent_cat.Parent == null).ToList();
                    ViewBag.CatName = "Оборудование";
                }
                model.Sub = cats;
                if (cats.Count == 0)
                    return products((int)parent, null);
            }


            return View(model);
        }

        public PartialViewResult filters(int cat_id)
        {

            List<AttributesValues> attributes = null;
            using (var context = new CodeContext())
            {
                context.Attributes.ToList();
                context.Products.ToList();
                Category cat = context.Categories.First(c => c.Id == cat_id);
                attributes = context.AttributesValues
                    .Where(attr => (attr.Attribute.Category.Id == cat_id) && attr.Attribute.Status == true)
                    .ToList();
                
                
            }
            return PartialView(attributes);
        }

        public ActionResult products(int cat_id, Category cat_model)
        {
            Category model = cat_model == null ? new Category() : cat_model;
            var context = new CodeContext();

            if (model.Products == null)
                model.Products = context.Products.Where(p => p.Category.Id == cat_id).ToList();
            context.Attributes.ToList();
            context.Products.ToList();
            Category cat = context.Categories.First(c => c.Id == cat_id);
            model.Attributes = context.AttributesValues
                .Where(attr => (attr.Attribute.Category.Id == cat_id) && attr.Attribute.Status == true)
                .ToList();
            List<Filters> filters = new List<Filters>();
                Filters filter;
                foreach(var attr in model.Attributes.Select(a=>a.Attribute).Distinct())
                {
                    filter = new Filters();
                    filter.Attribute = attr;
                    filter.Max_Value = model.Attributes.Where(a => a.Attribute == attr).Select(v => v.Double_Value).Max();
                    filter.Min_Value = model.Attributes.Where(a => a.Attribute == attr).Select(v => v.Double_Value).Min();
                    filter.String_Values = model.Attributes.Where(a => a.Attribute == attr).Select(v => v.String_Value).ToList();

                    filters.Add(filter);
                }

                ViewBag.Filters = filters;

                return View(model);

            

        }
        public PartialViewResult CatalogueOfProducts(int cat_id)
        {
            Category model = null;
            using (var context = new CodeContext())
            {
                model = context.Categories.Find(cat_id);
                context.Attributes.ToList();
                model.Attributes = context.AttributesValues
                    .Where(attr => (attr.Attribute.Category.Id == cat_id) && attr.Attribute.Status == true)
                    .ToList();
            }
            var products_models = model.Products.Select(m => m.Models);
            string models_id = "";
            foreach (var models in products_models)
            {
                foreach (var mod in models)
                {
                    models_id += mod.Id.ToString() + ";";
                }
            }
            ViewData["Models"] = models_id;
            return PartialView(model);
        }

        [HttpPost]
        public PartialViewResult CatalogueOfProducts(double? Max_Value, double? Min_Value, List<String> String_Values, int attr_id, int cat_id, string models)
        {

            Category model = SelectModels(Max_Value, Min_Value, String_Values, attr_id, cat_id, models);

            return PartialView(model);

        }

        public ActionResult product(int product_id, List<int> selected_models)
        {
            var context = new CodeContext();
            Product model = context.Products.Find(product_id);

            List<Model> selected = context.Models.Where(m => selected_models.Contains(m.Id)).ToList();
            ViewBag.SelectedModels = selected;

            return View(model);
        }

        public Category SelectModels(double? Max_Value, double? Min_Value, List<String> String_Values, int attr_id, int cat_id, string models)
        {
            Category model = null;
            if (Max_Value == null) Max_Value = 0;
            if (Min_Value == null) Min_Value = 0;
            string[] models_id = models.Split(';');
            var context = new CodeContext();

            context.Attributes.ToList();
            context.Categories.ToList();
            context.Products.ToList();
            context.Models.ToList();
            context.AttributesValues.ToList();
            Filters attribute = new Filters() { Max_Value = (double)Max_Value, Min_Value = (double)Min_Value, String_Values = String_Values };
            attribute.Attribute = context.Attributes.Where(a => a.id == attr_id).First();
            model = context.Categories.Find(cat_id);
            model.Attributes = context.AttributesValues
                .Where(attr => (attr.Attribute.Category.Id == cat_id) && attr.Attribute.Status == true)
                .ToList();
            List<Product> products = context.Products.ToList();
            List<Model> models_prods = new List<Model>();

            for (int i = 0; i < models_id.Length - 1; i++)
                models_prods.Add(context.Models.Find(Convert.ToDouble(models_id[i])));

            model.Products = new List<Product>();
            foreach (var prod in products)
            {
                prod.Models = prod.Models.Where(m => models_prods.Contains(m)).ToList();
            }

            products = products.Where(p => p.Models.Count != 0).ToList();
            foreach (var product in products)
            {

                if (attribute.Attribute.Type == Types.Double)
                    product.Models = product.Models
                        .Where(m => m.Properties.Where(p => p.Attribute == attribute.Attribute).FirstOrDefault().Double_Value <= attribute.Max_Value
                        && m.Properties.Where(p => p.Attribute == attribute.Attribute).FirstOrDefault().Double_Value >= attribute.Min_Value).ToList();
                if (attribute.Attribute.Type == Types.String)
                {
                    if (String_Values != null)
                    {
                        try
                        {
                            if (product.Models.Select(m => m.Properties.Where(p => p.Attribute == attribute.Attribute)).Count() != 0)
                                product.Models = product.Models.Where(m => String_Values.Contains(m.Properties.Where(p => p.Attribute == attribute.Attribute)
                            .FirstOrDefault().String_Value)).ToList();
                            else
                                product.Models = new List<Model>();
                        }
                        catch (Exception e) { }
                    }

                }
            }
            model.Products = products.Where(p => p.Models.Count != 0).ToList();
            return model;
        }

    }
}