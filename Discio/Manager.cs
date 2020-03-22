using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Discio {
    public class Manager<T> where T : IDiscio {
        private readonly string master;

        /// <summary>
        /// Data manager
        /// </summary>
        /// <param name="name">Master</param>
        public Manager(string name) :
            this(name, "") { }


        /// <summary>
        /// Data manager
        /// </summary>
        /// <param name="name">Master</param>
        /// <param name="source">Storage source</param>
        public Manager(string name, Source source) {
            if (source == null) {
                source = SiteSources.Default;
            }

            Source src = source ?? new Source();

            toInsert = new List<T>();
            toUpdate = new List<T>();
            toDelete = new List<T>();

            if (src.IsValid(name)) {
                Storage = src.StoragePath(name);
                master = src.MasterPath(name);
            }
            else {
                throw new DiscioException("Invalid DataFolder in source");
            }

        }

        /// <summary>
        /// Data manager
        /// </summary>
        /// <param name="name">Master</param>
        /// <param name="sourceKey">Storage source key</param>
        public Manager(string name, string sourceKey) : this(name,
            (!string.IsNullOrEmpty(sourceKey)) ? SiteSources.Sources[sourceKey] : null) {
        }

        public void Commit() {
            InsertItems();
            UpdateItems();
            DeleteItems();
        }

        private static string ToJson(List<T> items) {
            return JsonConvert.SerializeObject(items);
        }

        private void ToStorage(List<T> items) {
            string json = ToJson(items);
            if (File.Exists(master)) {
                File.WriteAllText(master, json);
            }

        }

        private List<T> FromStorage() {
            var r = new List<T>();

            if (!File.Exists(master)) return r;
            string json = File.ReadAllText(master);
            r = JsonConvert.DeserializeObject<List<T>>(json) ?? new List<T>();

            return r;
        }

        #region Insert Data

        private void CreateIds() {
            foreach (T item in toInsert) {
                ((IDiscio)item).ID = Guid.NewGuid().ToString();
            }
        }

        private void InsertItems() {
            if (toInsert == null || toInsert.Count <= 0) return;
            CreateIds();

            List<T> l = Select();

            l.AddRange(toInsert);


            ToStorage(l);
            toInsert.Clear();
        }

        private void UpdateItems() {
            if (toUpdate == null || toUpdate.Count <= 0) return;
            List<T> l = Select();

            foreach (T item in toUpdate) {
                int index = l.FindIndex(m => ((IDiscio)m).ID == ((IDiscio)item).ID);
                if (index != -1) {
                    l[index] = item;
                }
            }

            ToStorage(l);
            toUpdate.Clear();
        }

        private void DeleteItems() {
            if (toDelete == null || toDelete.Count <= 0) return;
            List<T> l = Select();

            foreach (T item in toDelete) {
                l.RemoveAll(m => ((IDiscio)m).ID == ((IDiscio)item).ID);
            }

            ToStorage(l);

            toDelete.Clear();
        }

        #endregion


        #region "Queries"



        /// <summary>
        /// Is any record matching the criteria exists
        /// </summary>
        /// <param name="wherePredicate"></param>
        /// <returns></returns>
        public bool Any(Func<T, bool> wherePredicate) {
            List<T> l = FromStorage();
            return wherePredicate != null && l.Any(wherePredicate);
        }

        /// <summary>
        /// Select list of records using a "wherePredicate" predicate
        /// </summary>
        /// <param name="wherePredicate">predicate</param>
        /// <returns>List</returns>
        public List<T> Select(Func<T, bool> wherePredicate) {
            List<T> l = FromStorage();

            return wherePredicate != null ? l.Where(wherePredicate).ToList() : l;
        }

        public string SelectJson() => File.ReadAllText(master);

        public string SelectJson(Func<T, bool> wherePredicate) => ToJson(Select(wherePredicate));

        /// <summary>
        /// Select all records in the dataset
        /// </summary>
        /// <returns>List</returns>
        public List<T> Select() => Select(null);


        /// <summary>
        /// Return first record
        /// </summary>
        /// <returns>T</returns>
        public T First() {
            List<T> l = FromStorage();
            return l.FirstOrDefault();
        }

        /// <summary>
        /// Return first record
        /// </summary>
        /// <param name="wherePredicate">predicate</param>
        /// <returns>T</returns>
        public T First(Func<T, bool> wherePredicate) {
            List<T> l = Select(wherePredicate);
            return l.FirstOrDefault();
        }

        /// <summary>
        /// Return last record
        /// </summary>
        /// <returns>T</returns>
        public T Last() {
            List<T> l = FromStorage();
            return l.LastOrDefault();
        }

        /// <summary>
        /// Return last record
        /// </summary>
        /// <param name="wherePredicate">predicate</param>
        /// <returns>T</returns>
        public T Last(Func<T, bool> wherePredicate) {
            List<T> l = Select(wherePredicate);
            return l.LastOrDefault();
        }

        /// <summary>
        /// Return top N values
        /// </summary>
        /// <param name="n">Number of values</param>
        /// <returns>List of T</returns>
        public List<T> Top(int n) {
            List<T> l = FromStorage();
            return l.OrderByDescending(m => m.ID).Take(n).ToList();
        }

        /// <summary>
        /// Select a page from a set with default size or 10
        /// </summary>
        /// <param name="page">Page number</param>
        /// <returns>List</returns>
        public List<T> Page(int page) => Page(page, 10);

        /// <summary>
        /// Select a page from a dataset
        /// </summary>
        /// <param name="page">Page number</param>
        /// <param name="size">Page size</param>
        /// <returns>List</returns>
        public List<T> Page(int page, int size) {
            List<T> l = Select();
            return l.Skip((page - 1) * size).Take(size).ToList();
        }


        /// <summary>
        /// Select a page from a dataset
        /// </summary>
        /// <param name="page">Page number</param>
        /// <param name="size">Page size</param>
        /// <param name="wherePredicate">Predicate</param>
        /// <returns>List</returns>
        public List<T> Page(int page, int size, Func<T, bool> wherePredicate) {
            List<T> l = Select(wherePredicate);
            return l.Skip((page - 1) * size).Take(size).ToList();
        }

        /// <summary>
        /// Select a page in descending order from a dataset
        /// </summary>
        /// <param name="page">Page number</param>
        /// <param name="size">Page size</param>
        /// <returns>List</returns>
        public List<T> PageDescending(int page, int size) {
            List<T> l = Select().OrderByDescending(m => m.ID).ToList();
            return l.Skip((page - 1) * size).Take(size).ToList();
        }

        /// <summary>
        /// Select a page in descending order from a set with default size or 10
        /// </summary>
        /// <param name="page">Page number</param>
        /// <returns>List</returns>
        public List<T> PageDescending(int page) => PageDescending(page, 10);

        /// <summary>
        /// Select a page in descending order from a dataset
        /// </summary>
        /// <param name="page">Page number</param>
        /// <param name="size">Page size</param>
        /// <param name="wherePredicate">Predicate</param>
        /// <returns>List</returns>
        public List<T> PageDescending(int page, int size, Func<T, bool> wherePredicate) {
            List<T> l = Select().OrderByDescending(m => m.ID).ToList();
            return l.Skip((page - 1) * size).Take(size).ToList();
        }

        /// <summary>
        /// Return number of pages by size
        /// </summary>
        /// <param name="size">Size</param>
        /// <returns>count</returns>
        public int PageCount(int size) {
            double count = Count();
            return (int)Math.Ceiling((count / size));
        }

        /// <summary>
        /// Return number of pages by size
        /// </summary>
        /// <param name="size">Size</param>
        /// <param name="wherePredicate">Predicate</param>
        /// <returns>count</returns>
        public int PageCount(int size, Func<T, bool> wherePredicate) {
            double count = Count(wherePredicate);
            return (int)Math.Ceiling((count / size));
        }


        /// <summary>
        /// Load record by id
        /// </summary>
        /// <param name="id">Record id</param>
        /// <returns>Record</returns>
        public T Load(string id) {
            List<T> l = Select();
            return l.FirstOrDefault(m => ((IDiscio)m).ID == id);
        }

        /// <summary>
        /// Number of records in dataset
        /// </summary>
        /// <returns>Int</returns>
        public int Count() => Count(null);

        /// <summary>
        /// Number of records that match predicate
        /// </summary>
        /// <param name="wherePredicate">Predicate</param>
        /// <returns>int</returns>
        public int Count(Func<T, bool> wherePredicate) {
            List<T> l = Select(wherePredicate);

            return l.Count;
        }

        public void RemoveAll() => ToStorage(new List<T>());

        #endregion


        /// <summary>
        /// Insert an item
        /// </summary>
        /// <param name="item">IDiscio item</param>
        public void Insert(T item) {
            if (item != null) {
                toInsert.Add(item);
            }

        }

        /// <summary>
        /// Insert a list of items
        /// </summary>
        /// <param name="items">IDiscio items</param>
        public void Insert(List<T> items) {
            if (items != null && items.Count > 0) {
                toInsert.AddRange(items);
            }
        }

        /// <summary>
        /// Add an item to be deleted
        /// </summary>
        /// <param name="item">IDiscio item</param>
        public void Delete(T item) {
            if (item != null) {
                toDelete.Add(item);
            }
        }

        public void Delete(Func<T, bool> where) {
            List<T> s = Select(where);
            if (s.Count > 0) {
                Delete(s);
            }
        }

        /// <summary>
        /// Add items to be deleted
        /// </summary>
        /// <param name="items">IDiscio items</param>
        public void Delete(List<T> items) {
            if (items != null && items.Count > 0) {
                toDelete.AddRange(items);
            }
        }

        /// <summary>
        /// Add an item to be updated
        /// </summary>
        /// <param name="item">IDiscio item</param>
        public void Update(T item) {
            if (item != null) {
                toUpdate.Add(item);
            }
        }

        /// <summary>
        /// Add items to be updated
        /// </summary>
        /// <param name="items">IDiscio items</param>
        public void Update(List<T> items) {
            if (items != null && items.Count > 0) {
                toUpdate.AddRange(items);
            }

        }

        private readonly List<T> toInsert;
        private readonly List<T> toUpdate;
        private readonly List<T> toDelete;

        public string Storage { set; get; }

    }
}
