﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ShopatBanaturalPortal.Models;
using System.Text;
using System.Text.RegularExpressions;

namespace ShopatBanaturalPortal.Controllers
{
    public class InventoryItemsController : Controller
    {
        private InventoryItemDbContext db = new InventoryItemDbContext();

        // GET: InventoryItems
        public ActionResult Index(string searchString = "")
        {
            if(searchString != null)
            {
                ViewBag.SearchString = searchString;
            }
            

            var ItemsSelected = from m in db.InventoryItemDatabase
                                  orderby m.Type, m.CustomID
                                  select m;



            if (!String.IsNullOrEmpty(searchString))
            {
                ItemsSelected = from m in db.InventoryItemDatabase
                                where m.ItemName.Contains(searchString)  || m.Type.Contains(searchString)  || (m.QuantityLeft.ToString()).Contains(searchString) || m.Brand.Contains(searchString) || m.CustomID.Contains(searchString) || m.GeneralDescription.Contains(searchString)
                                orderby m.Type, m.QuantityLeft
                                select m;
            }

            return View(ItemsSelected);
        }

        public ActionResult ExportExcel(string searchString)
        {
            searchString = searchString + "";

            var ItemsSelected = (from m in db.InventoryItemDatabase
                                 orderby m.Type, m.CustomID
                                 select m).ToList();



            if (!String.IsNullOrEmpty(searchString))
            {
                ItemsSelected = (from m in db.InventoryItemDatabase
                                where m.ItemName.Contains(searchString) || m.Type.Contains(searchString) || (m.QuantityLeft.ToString()).Contains(searchString) || m.Brand.Contains(searchString) || m.CustomID.Contains(searchString) || m.GeneralDescription.Contains(searchString)
                                orderby m.Type, m.QuantityLeft
                                select m).ToList();
            }

            List<InventoryItem> ReadyToExport = ItemsSelected;
            TempData["ReadyToExportSelection"] = ReadyToExport;
            return RedirectToAction("ExportToExcel");
        }

        [HttpGet]
        public FileContentResult ExportToExcel()
        {
            List<InventoryItem> ReadyToexport = new List<InventoryItem>();
            ReadyToexport = (List<InventoryItem>)TempData["ReadyToExportSelection"];
            string[] columns = { "ItemName", "Brand", "CustomID", "QuantityLeft", "Price", "Type", "GeneralDescription", "SoldtoDate" };
            byte[] filecontent = ExcelExportHelper.ExportExcel(ReadyToexport, "Inventory", false, columns);
            return File(filecontent, ExcelExportHelper.ExcelContentType, "Inventory_Report_"+DateTime.Now.DayOfYear + "_"+ DateTime.Now.Year +".xlsx");
        }
        public ActionResult ChartsIndex(string searchString)
        {

            var ItemsSelected = from m in db.InventoryItemDatabase
                                orderby m.Type, m.CustomID
                                select m;



            if (!String.IsNullOrEmpty(searchString))
            {
                ItemsSelected = from m in db.InventoryItemDatabase
                                where m.ItemName == searchString || m.Type == searchString || (m.QuantityLeft.ToString()).Contains(searchString) || m.Brand == searchString || m.CustomID == searchString
                                orderby m.Type, m.QuantityLeft
                                select m;
            }

            return View(ItemsSelected);
        }

        public ActionResult TransactionHistory(int ID)
        {
            var SelectedItem = from S in db.InventoryItemDatabase
                               where S.ID == ID
                               select S;
            InventoryItem ItemModel = (InventoryItem)SelectedItem.First<InventoryItem>();

            string[] History = ItemModel.TransactionHistory.Split('*');
            ViewBag.Model = ItemModel;
            return View(History);
        }
        public ActionResult ShipmentHistory(int ID)
        {
            var SelectedItem = from S in db.InventoryItemDatabase
                               where S.ID == ID
                               select S;
            InventoryItem ItemModel = (InventoryItem)SelectedItem.First<InventoryItem>();

            string[] History = ItemModel.ShipmentHistory.Split('*');
            return View(History);
        }

        // GET: InventoryItems/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InventoryItem inventoryItem = db.InventoryItemDatabase.Find(id);
            if (inventoryItem == null)
            {
                return HttpNotFound();
            }
            return View(inventoryItem);
        }

        // GET: InventoryItems/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: InventoryItems/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(string Type, [Bind(Include = "ID,CustomID,Brand,ItemName,Price,QuantityLeft,Type,GeneralDescription")] InventoryItem inventoryItem)
        {
            if (ModelState.IsValid)
            {
                inventoryItem.Type = Type;
                inventoryItem.LastShipmentRecieved = "Not Yet Stocked";
                inventoryItem.ItemNumber = inventoryItem.ID;
                inventoryItem.ItemName = inventoryItem.ItemName.ToUpper();
                inventoryItem.Brand = inventoryItem.Brand.ToUpper();

                db.InventoryItemDatabase.Add(inventoryItem);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(inventoryItem);
        }

        // GET: InventoryItems/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InventoryItem inventoryItem = db.InventoryItemDatabase.Find(id);
            if (inventoryItem == null)
            {
                return HttpNotFound();
            }
            return View(inventoryItem);
        }

        // POST: InventoryItems/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,CustomID,Brand,ItemName,ItemNumber,SoldtoDate,SoldthisMonth,SoldthisYear,QuantityLeft,LastShipmentRecieved,Type,ShipmentHistory,GeneralDescription,TransactionHistory,Price")] InventoryItem inventoryItem)
        {
            if (ModelState.IsValid)
            {
                db.Entry(inventoryItem).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(inventoryItem);
        }

        

        public ActionResult UpdateShipments(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InventoryItem inventoryItem = db.InventoryItemDatabase.Find(id);
            if (inventoryItem == null)
            {
                return HttpNotFound();
            }
            return View(inventoryItem);
        }

        // POST: InventoryItems/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateShipments(int ID, string Restock)
        {
            var SelectedItem = from S in db.InventoryItemDatabase
                               where S.ID == ID
                               select S;
            InventoryItem ItemModel = (InventoryItem)SelectedItem.First<InventoryItem>();

            if (ModelState.IsValid)
            {
                ItemModel.LastShipmentRecieved = DateTime.Now.ToShortDateString();
                ItemModel.QuantityLeft = ItemModel.QuantityLeft + Convert.ToInt32(Restock);
                //split by **
                ItemModel.ShipmentHistory = ItemModel.ShipmentHistory + "[Restocked] " + Restock + " " + ItemModel.ItemName + ". [Restocked on] " + DateTime.Now.ToShortDateString() + "*";


                db.Entry(ItemModel).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(ItemModel);
        }

        public ActionResult ItemSold(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InventoryItem inventoryItem = db.InventoryItemDatabase.Find(id);
            if (inventoryItem == null)
            {
                return HttpNotFound();
            }
            return View(inventoryItem);
        }

        // POST: InventoryItems/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ItemSold(int ID, string Sold)
        {
            var SelectedItem = from S in db.InventoryItemDatabase
                               where S.ID == ID
                               select S;

            InventoryItem ItemModel = (InventoryItem)SelectedItem.First<InventoryItem>();

            if (ModelState.IsValid)
            {
                ItemModel.QuantityLeft = ItemModel.QuantityLeft - Convert.ToInt32(Sold);
                ItemModel.SoldtoDate = ItemModel.SoldtoDate + Convert.ToInt32(Sold);
                //split by **
                ItemModel.TransactionHistory = ItemModel.TransactionHistory + "[Sold] " + Sold + " " + ItemModel.ItemName + ". [Date] " + DateTime.Now.ToLocalTime() + ". [Revenue] " + (Convert.ToInt32(Sold) * ItemModel.Price) + " $" + "*";
                db.Entry(ItemModel).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(ItemModel);
        }

        // GET: InventoryItems/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InventoryItem inventoryItem = db.InventoryItemDatabase.Find(id);
            if (inventoryItem == null)
            {
                return HttpNotFound();
            }
            return View(inventoryItem);
        }

        // POST: InventoryItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            InventoryItem inventoryItem = db.InventoryItemDatabase.Find(id);
            db.InventoryItemDatabase.Remove(inventoryItem);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public ActionResult GenerateChart(int ID)
        {
            var SelectedItem = from S in db.InventoryItemDatabase
                               where S.ID == ID
                               select S;
            InventoryItem ItemModel = (InventoryItem)SelectedItem.First<InventoryItem>();

            string[] Chart_Intermediate = ItemModel.TransactionHistory.Split(new string[] { "[Date]" }, StringSplitOptions.None);
            ViewBag.Model = ItemModel;
            List<string> DateList = new List<string>();
            List<string> SoldList = new List<string>();

            Regex dateRegex = new Regex(@"\b\d{1,2}\/\d{1,2}\/\d{4}\b");
            Match match;

            foreach (var item in Chart_Intermediate)
            {
               match = null;
               match = dateRegex.Match(item);
                var observe = match.ToString();

                if (match != null && match.ToString() != string.Empty)
                {
                    DateList.Add(match.ToString());
                }
            }

            return View(DateList);
        }
    }
}
