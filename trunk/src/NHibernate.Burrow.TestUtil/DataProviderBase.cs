using System;
using System.Collections.Generic;
using NHibernate.Burrow;
using NHibernate.Burrow.NHDomain;
using NHibernate;
using NUnit.Framework;

namespace NHibernate.Burrow.TestUtil {
    [TestFixture]
    public class DataProviderBase {
        private readonly LinkedList<object> createdData = new LinkedList<object>();
        
        public readonly Random Random = new Random(19);

        public static string RandomName() {
            return RandomString(5);
        }

        protected static string RandomEmail()
        {
            return RandomName() + "@"+ RandomName() + ".com";
        }

        public static string RandomString(int length) {
            return RandomStringGenerator.GenerateLetterStrings(length);
        }

        public static int RandomInt() {
            return Math.Abs(RandomStringGenerator.GenerateLetterStrings(10).GetHashCode()/100);
        }

        protected void AddCreatedData(object  o) {
            createdData.AddFirst(o);
        }

        /// <summary>
        /// Delete the test data in a FIFO fashion
        /// </summary>
        public void ClearData() {
            LinkedListNode<object> node = createdData.First;
            while (node != null) {
                object o = node.Value;
                DeleteEntity(o);
                node = node.Next;
            }
            createdData.Clear();
        }

        public static void DeleteEntity(object o) {
            if (o == null) return;
            if (o is IPersistantObjWithDAO)
                DeletePersistantObject((IPersistantObjWithDAO) o);
            else if (o is IPersistantObjSaveDelete)
                DeletePersistantObject((IPersistantObjSaveDelete) o);
            else  
                DeletePersistantObject(o);
           
        }

        protected static void DeletePersistantObject(IPersistantObjWithDAO o) {
            o = (IPersistantObjWithDAO)ReloadIWithId(o);
            if (o != null)
            o.DAO.Delete();
        }

        private static object ReloadIWithId(IWithId o) {
            return GetSession(o.GetType()).Get(o.GetType(), o.Id);
        }

        private static ISession GetSession(System.Type t) {
            return SessionManager.GetInstance(t).GetSession();
        }

        protected static void DeletePersistantObject(IPersistantObjSaveDelete o) {
            o = (IPersistantObjSaveDelete)ReloadIWithId(o);
            if (o != null)
            o.Delete();
        } 
        
        protected static void DeletePersistantObject(object o) {
            o = GetSession(o.GetType()).Get(o.GetType(), GetEntityId(o));
            if(o != null)
               GetSession(o.GetType()).Delete(o);
        }


        private static object GetEntityId(object entity)
        {
            return NHDomain.Loader.Instance.GetId(entity);
        }
    }
}