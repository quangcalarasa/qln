//using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Text;

namespace IOITQln.Common.ViewModels.Firestore
{
    //[FirestoreData]
    public class DtoUserFirestore
    {
        //[FirestoreProperty]
        public long UserId { get; set; }
        //[FirestoreProperty]
        public int Status { get; set; }
        //[FirestoreProperty]
        public string UserName { get; set; }
        //[FirestoreProperty]
        public string FullName { get; set; }
        //[FirestoreProperty]
        public string Phone { get; set; }
        //[FirestoreProperty]
        public string Avatar { get; set; }
        //[FirestoreProperty]
        public string DateAction { get; set; }
        //[FirestoreProperty]
        public int CountNotification { get; set; }

        //public void Dispose()
        //{
        //    GC.SuppressFinalize(this);
        //}
    }
}
