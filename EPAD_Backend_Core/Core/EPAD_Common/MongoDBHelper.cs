using EPAD_Common.Types;
using EPAD_Data.Models;
using Microsoft.Extensions.Caching.Memory;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPAD_Common
{
    public class MongoDBHelper<T>
    {
        private MongoClient mClient;
        private IMongoDatabase mDatabase;
        IMongoCollection<T> mCollection;
        string mTableName = "";
        private bool mUseMongoDB = false;

        public MongoDBHelper(string pCollection, IMemoryCache pCache)
        {
            if (mClient == null)
            {
                ConfigObject config = ConfigObject.GetConfig(pCache);
                if (config.MongoDBConnectionString == "")
                {
                    return;
                }
                mUseMongoDB = true;
                mClient = new MongoClient(config.MongoDBConnectionString);
                
                mTableName = pCollection;
                mDatabase = mClient.GetDatabase("EPADLog");
                mCollection = mDatabase.GetCollection<T>(pCollection);
            }
        }

        public bool CheckUseMongoDB()
        {
            return mUseMongoDB;
        }

        public void AddDataToCollection(T dataInsert, bool isAsync)
        {
            if (mUseMongoDB == false)
            {
                return;
            }
            if (mCollection != null)
            {
                if (isAsync == false)
                {
                    mCollection.InsertOne(dataInsert);
                }
                else
                {
                    mCollection.InsertOneAsync(dataInsert);
                }
            }
        }

        public void AddListDataToCollection(List<T> dataInsert, bool isAsync)
        {
            if (mUseMongoDB == false)
            {
                return;
            }
            if (mCollection != null)
            {
                if (isAsync == false)
                {
                    mCollection.InsertMany(dataInsert);
                }
                else
                {
                    mCollection.InsertManyAsync(dataInsert);
                }
            }
        }

        public List<T> GetListData()
        {
            List<T> listData = mCollection.Find<T>(t => true).Limit(1000).ToList();
            return listData;
        }

        public IMongoCollection<T> GetCollection()
        {
            return mCollection;
        }

        private bool CheckCollectionExist(string pTableName)
        {
            var filter = new BsonDocument("name", pTableName);
            var collection = mDatabase.ListCollections(new ListCollectionsOptions { Filter = filter });

            if (collection.Any())
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}