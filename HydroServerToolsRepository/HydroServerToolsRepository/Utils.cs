﻿using AutoMapper;
using HydroserverToolsBusinessObjects;
using HydroserverToolsBusinessObjects.Models;
using HydroServerToolsRepository.Repository;
using ODM_1_1_1EFModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity.Core;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace HydroServerToolsRepository.Repository
{
    public class RepositoryUtils
    {
        public static EntityKey GetEntityKey(EntitySet entitySet, dynamic d)
        {
            //check if entry with this key exists
            var entityKeyValues = new List<KeyValuePair<string, object>>();
            foreach (var member in entitySet.ElementType.KeyMembers)
            {
                var info = d.GetType().GetProperty(member.Name);
                var tempValue = info.GetValue(d, null);
                var pair = new KeyValuePair<string, object>(member.Name, tempValue);
                entityKeyValues.Add(pair);
            }
            var key = new EntityKey(entitySet.EntityContainer.Name + "." + entitySet.Name, entityKeyValues);
            return key;
        }

        public static DataTable ListToDataTable<T>(IList<T> list)
        {
            var table = new DataTable();

            return table;
        }

        public static void CommitNewRecords<T>(string entityConnectionString, string id, IList<T> list)
        {

            string providerConnectionString = new EntityConnectionStringBuilder(entityConnectionString).ProviderConnectionString;

            try 
            { 
                if (id == "sites")
                {
                    var recordsToInsert = new List<Site>();
                    foreach (T item in list)
                    {
                        var model = Mapper.Map< T,Site>(item);
                        recordsToInsert.Add(model);
                    }
                    BulkInsert<Site>(providerConnectionString, id, recordsToInsert);                   
                }

               
                if (id == "variables")
                {
                    var recordsToInsert = new List<Variable>();
                    foreach (T item in list)
                    {
                        var model = Mapper.Map<T, Variable>(item);
                        recordsToInsert.Add(model);
                    }
                    BulkInsert<Variable>(providerConnectionString, id, recordsToInsert);
                }
                if (id == "offsettypes")
                {
                    var recordsToInsert = new List<OffsetType>();
                    foreach (T item in list)
                    {
                        var model = Mapper.Map<T, OffsetType>(item);
                        recordsToInsert.Add(model);
                    }
                    BulkInsert<OffsetType>(providerConnectionString, id, recordsToInsert);
                }
                if (id == "sources")
                {
                    var context = new ODM_1_1_1EFModel.ODM_1_1_1Entities(entityConnectionString);
                    var objContext = ((IObjectContextAdapter)context).ObjectContext;
                    var sourcerecordsToInsert = new List<Source>();
                    var isometadatarecordsToInsert = new List<ISOMetadata>();
                    //wrap in transaction in case something goes wrong
                    using (TransactionScope scope = new TransactionScope())
                    {
                        foreach (T item in list)
                        {
                            var isometadatamodel = Mapper.Map<T, ISOMetadata>(item);
                            //isometadatarecordsToInsert.Add(isometadatamodel);
                            context.ISOMetadatas.Add(isometadatamodel);
                            objContext.SaveChanges(false);
                            //var newItem = Convert.ChangeType(item, typeof(SourcesModel));
                            var source = Mapper.Map<T, Source>(item);
                            source.MetadataID = isometadatamodel.MetadataID;
                            context.Sources.Add(source);
                        }
                        //BulkInsert<ISOMetadata>(providerConnectionString, id, isometadatarecordsToInsert);

                   
                        //BulkInsert<Source>(providerConnectionString, id, sourcerecordsToInsert);
                        objContext.SaveChanges(false);
                        //if we get here things are looking good.
                        scope.Complete();
                        objContext.AcceptAllChanges();                        
                    }                    
                }

                if (id == "methods")
                {
                    var recordsToInsert = new List<Method>();
                    foreach (T item in list)
                    {
                        var model = Mapper.Map<T, Method>(item);
                        recordsToInsert.Add(model);
                    }
                    BulkInsert<Method>(providerConnectionString, id, recordsToInsert);

                }

                if (id == "labmethods")
                {
                    var recordsToInsert = new List<LabMethod>();
                    foreach (T item in list)
                    {
                        var model = Mapper.Map<T, LabMethod>(item);
                        recordsToInsert.Add(model);
                    }
                    BulkInsert<LabMethod>(providerConnectionString, id, recordsToInsert);

                }

                if (id == "samples")
                {
                    var recordsToInsert = new List<Sample>();
                    foreach (T item in list)
                    {
                        var model = Mapper.Map<T, Sample>(item);
                        recordsToInsert.Add(model);
                    }
                    BulkInsert<Sample>(providerConnectionString, id, recordsToInsert);

                }

                if (id == "qualifiers")
                {
                    var recordsToInsert = new List<Qualifier>();
                    foreach (T item in list)
                    {
                        var model = Mapper.Map<T, Qualifier>(item);
                        recordsToInsert.Add(model);
                    }
                    BulkInsert<Qualifier>(providerConnectionString, id, recordsToInsert);

                }

                if (id == "qualitycontrollevels")
                {
                    var recordsToInsert = new List<QualityControlLevel>();
                    foreach (T item in list)
                    {
                        var model = Mapper.Map<T, QualityControlLevel>(item);
                        recordsToInsert.Add(model);
                    }
                    BulkInsert<QualityControlLevel>(providerConnectionString, id, recordsToInsert);

                }

                if (id == "datavalues")
                {
                    var recordsToInsert = new List<DataValue>();
                    foreach (T item in list)
                    {
                        var model = Mapper.Map<T, DataValue>(item);
                        recordsToInsert.Add(model);
                    }
                    BulkInsert<DataValue>(providerConnectionString, id, recordsToInsert);

                }

                if (id == "groupdescriptions")
                {
                    var recordsToInsert = new List<GroupDescription>();
                    foreach (T item in list)
                    {
                        var model = Mapper.Map<T, GroupDescription>(item);
                        recordsToInsert.Add(model);
                    }
                    BulkInsert<GroupDescription>(providerConnectionString, id, recordsToInsert);

                }

                if (id == "groups")
                {
                    var recordsToInsert = new List<Group>();
                    foreach (T item in list)
                    {
                        var model = Mapper.Map<T, Group>(item);
                        recordsToInsert.Add(model);
                    }
                    BulkInsert<Group>(providerConnectionString, id, recordsToInsert);
                }

                if (id == "derivedfrom")
                {
                    var recordsToInsert = new List<DerivedFrom>();
                    foreach (T item in list)
                    {
                        var model = Mapper.Map<T, DerivedFrom>(item);
                        recordsToInsert.Add(model);
                    }
                    BulkInsert<DerivedFrom>(providerConnectionString, id, recordsToInsert);
                }

                if (id == "categories")
                {
                    var recordsToInsert = new List<Category>();
                    foreach (T item in list)
                    {
                        var model = Mapper.Map<T, Category>(item);
                        recordsToInsert.Add(model);
                    }
                    BulkInsert<Category>(providerConnectionString, id, recordsToInsert);
                }
           }
            catch (Exception ex)
            {
                throw;
            }
   
        }

        public static void CommitUpdateRecords<T>(string entityConnectionString, string id, IList<T> list)
        {

            string providerConnectionString = new EntityConnectionStringBuilder(entityConnectionString).ProviderConnectionString;

            try
            {
                if (id == "sites")
                {
                    var recordsToInsert = new List<Site>();
                    var context = new ODM_1_1_1EFModel.ODM_1_1_1Entities(entityConnectionString);
                    foreach (T item in list)
                    {
                        var model = Mapper.Map<T, Site>(item);
                        Site existingItem = context.Sites.Where(a => a.SiteCode == model.SiteCode).FirstOrDefault();
                        
                        if (existingItem != null)
                        {
                            
                            existingItem.SiteName = model.SiteName; 
                            existingItem.Latitude= model.Latitude;  
                            existingItem.Longitude = model.Longitude;  
                            existingItem.LatLongDatumID = model.LatLongDatumID;  
                            existingItem.Elevation_m = model.Elevation_m;   
                            existingItem.VerticalDatum = model.VerticalDatum;  
                            existingItem.LocalX = model.LocalX; 
                            existingItem.LocalY = model.LocalY;
                            existingItem.LocalProjectionID = model.LocalProjectionID;
                            existingItem.PosAccuracy_m = model.PosAccuracy_m;
                            existingItem.State = model.State;
                            existingItem.County = model.County;
                            existingItem.Comments = model.Comments;
                            existingItem.SiteType = model.SiteType;

                            //context.Sites.Add(existingItem);
                        }
                        
                    }
                    context.SaveChanges();
                   
                }


                if (id == "variables")
                {
                    var recordsToInsert = new List<Variable>();
                    var context = new ODM_1_1_1EFModel.ODM_1_1_1Entities(entityConnectionString);
                    foreach (T item in list)
                    {
                        var model = Mapper.Map<T, Variable>(item);
                            var existingItem = context.Variables.Where(a => a.VariableCode == model.VariableCode).FirstOrDefault();

                        if (existingItem != null)
                        {

                            existingItem.VariableCode = model.VariableCode;
                            existingItem.VariableName = model.VariableName;
                            existingItem.Speciation = model.Speciation;
                            existingItem.VariableUnitsID = model.VariableUnitsID;
                            existingItem.SampleMedium = model.SampleMedium;
                            existingItem.ValueType = model.ValueType;
                            existingItem.IsRegular = model.IsRegular;
                            existingItem.TimeSupport = model.TimeSupport;
                            existingItem.TimeUnitsID = model.TimeUnitsID;
                            existingItem.DataType = model.DataType;
                            existingItem.GeneralCategory = model.GeneralCategory;
                            existingItem.NoDataValue = model.NoDataValue;
                           

                            //context.Sites.Add(existingItem);
                        }

                    }
                    context.SaveChanges();
                }
                if (id == "offsettypes")
                {
                    
                }
                if (id == "sources")
                {
                    var context = new ODM_1_1_1EFModel.ODM_1_1_1Entities(entityConnectionString);
                    var objContext = ((IObjectContextAdapter)context).ObjectContext;
                    var sourcerecordsToInsert = new List<Source>();
                    var isometadatarecordsToInsert = new List<ISOMetadata>();
                    //wrap in transaction in case something goes wrong
                    using (TransactionScope scope = new TransactionScope())
                    {
                        foreach (T item in list)
                        {
                            var isometadatamodel = Mapper.Map<T, ISOMetadata>(item);
                            //isometadatarecordsToInsert.Add(isometadatamodel);
                            context.ISOMetadatas.Add(isometadatamodel);
                            objContext.SaveChanges(false);
                            //var newItem = Convert.ChangeType(item, typeof(SourcesModel));
                            var source = Mapper.Map<T, Source>(item);
                            source.MetadataID = isometadatamodel.MetadataID;
                            context.Sources.Add(source);
                        }
                        //BulkInsert<ISOMetadata>(providerConnectionString, id, isometadatarecordsToInsert);


                        //BulkInsert<Source>(providerConnectionString, id, sourcerecordsToInsert);
                        objContext.SaveChanges(false);
                        //if we get here things are looking good.
                        scope.Complete();
                        objContext.AcceptAllChanges();
                    }
                }

                if (id == "methods")
                {
                    
                }

                if (id == "labmethods")
                {
                    

                }

                if (id == "samples")
                {
                    var recordsToInsert = new List<Sample>();
                    foreach (T item in list)
                    {
                        var model = Mapper.Map<T, Sample>(item);
                        recordsToInsert.Add(model);
                    }
                    BulkInsert<Sample>(providerConnectionString, id, recordsToInsert);

                }

                if (id == "qualifiers")
                {
                    

                }

                if (id == "qualitycontrollevels")
                {
                    var recordsToInsert = new List<QualityControlLevel>();
                    var context = new ODM_1_1_1EFModel.ODM_1_1_1Entities(entityConnectionString);
                    foreach (T item in list)
                    {
                        var model = Mapper.Map<T, QualityControlLevel>(item);
                        var existingItem = context.QualityControlLevels.Where(a => a.QualityControlLevelCode == model.QualityControlLevelCode).FirstOrDefault();

                        if (existingItem != null)
                        {

                            existingItem.Definition = model.Definition;
                            existingItem.Explanation = model.Explanation;                           

                           
                        }

                    }
                    context.SaveChanges();

                }

                if (id == "datavalues")
                {
                    var recordsToInsert = new List<DataValue>();
                    foreach (T item in list)
                    {
                        var model = Mapper.Map<T, DataValue>(item);
                        recordsToInsert.Add(model);
                    }
                    BulkInsert<DataValue>(providerConnectionString, id, recordsToInsert);

                }

                if (id == "groupdescriptions")
                {
                    var recordsToInsert = new List<GroupDescription>();
                    foreach (T item in list)
                    {
                        var model = Mapper.Map<T, GroupDescription>(item);
                        recordsToInsert.Add(model);
                    }
                    BulkInsert<GroupDescription>(providerConnectionString, id, recordsToInsert);

                }

                if (id == "groups")
                {
                    var recordsToInsert = new List<Group>();
                    foreach (T item in list)
                    {
                        var model = Mapper.Map<T, Group>(item);
                        recordsToInsert.Add(model);
                    }
                    BulkInsert<Group>(providerConnectionString, id, recordsToInsert);
                }

                if (id == "derivedfrom")
                {
                    var recordsToInsert = new List<DerivedFrom>();
                    foreach (T item in list)
                    {
                        var model = Mapper.Map<T, DerivedFrom>(item);
                        recordsToInsert.Add(model);
                    }
                    BulkInsert<DerivedFrom>(providerConnectionString, id, recordsToInsert);
                }

                if (id == "categories")
                {
                    var recordsToInsert = new List<Category>();
                    foreach (T item in list)
                    {
                        var model = Mapper.Map<T, Category>(item);
                        recordsToInsert.Add(model);
                    }
                    BulkInsert<Category>(providerConnectionString, id, recordsToInsert);
                }
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        public static void BulkInsert<T>(string connection, string tableName, IList<T> list)
        {
            try 
            { 
            //connection = "Data Source=tcp:bhi5g2ajst.database.windows.net,1433;Database=hydroservertest2;User ID=HisCentralAdmin@bhi5g2ajst;Password=f3deratedResearch;Integrated Security=false;Trusted_Connection=False;Encrypt=True;Connection Timeout=30;Persist Security Info = true";



            //bulkCopy.BatchSize = list.Count;
            // bulkCopy.DestinationTableName = tableName;


            var table = new DataTable();
            var props = TypeDescriptor.GetProperties(typeof(T))
                //Dirty hack to make sure we only have system data types 
                //i.e. filter out the relationships/collections
                                       .Cast<PropertyDescriptor>()
                                       .Where(propertyInfo => propertyInfo.PropertyType.Namespace.Equals("System"))
                                       .ToArray();
            var sortedProps = props.OrderBy(x => x.Name).ToArray();

            foreach (var propertyInfo in props)
            {
                if (propertyInfo.Name != "ValueID")
                {
                    //bulkCopy.ColumnMappings.Add(propertyInfo.Name, propertyInfo.Name);
                }
                table.Columns.Add(propertyInfo.Name, Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType);

            }

            var values = new object[props.Length];
            foreach (var item in list)
            {

                for (var i = 0; i < values.Length; i++)
                {
                    values[i] = props[i].GetValue(item);
                }

                table.Rows.Add(values);
            }



            // open the destination data
            using (SqlConnection destinationConnection =
                            new SqlConnection(connection))
            {
                // open the connection
                destinationConnection.Open();

                using (SqlBulkCopy bulkCopy =
                            new SqlBulkCopy(destinationConnection.ConnectionString, SqlBulkCopyOptions.KeepNulls | SqlBulkCopyOptions.CheckConstraints))
                {
                    //bulkCopy.SqlRowsCopied += new SqlRowsCopiedEventHandler(OnSqlRowsTransfer);
                    //bulkCopy.NotifyAfter = 100;
                    bulkCopy.BatchSize = 50;


                    // bulkCopy.ColumnMappings.Add("OrderID", "NewOrderID");     
                    bulkCopy.DestinationTableName = tableName;
                    bulkCopy.WriteToServer(table);
                }
            }
                }
            catch
            {
                throw;
            }
            //bulkCopy.WriteToServer(table);

        }

        public static bool containsInvalidCharacters(string value)
        {

            var a = (System.Text.RegularExpressions.Regex.Matches(value, @"[\040]").Count != 0);
            var b = (System.Text.RegularExpressions.Regex.Matches(value, @"[\,\+]").Count != 0);
            var c = (System.Text.RegularExpressions.Regex.Matches(value, @"[\:\\/\=]").Count != 0);
            var d = (System.Text.RegularExpressions.Regex.Matches(value, @"[\t\r\v\f\n]").Count != 0);

            bool hasInvalidCharacters;
            hasInvalidCharacters = ((System.Text.RegularExpressions.Regex.Matches(value, @"[\040]").Count != 0) ||
                                        (System.Text.RegularExpressions.Regex.Matches(value, @"[\,\+]").Count != 0) ||
                                        (System.Text.RegularExpressions.Regex.Matches(value, @"[\:\\/\=]").Count != 0) ||
                                        (System.Text.RegularExpressions.Regex.Matches(value, @"[\t\r\v\f\n]").Count != 0));
            return hasInvalidCharacters;
        }

        public static bool containsSpecialCharacters(string value)
        {
            bool hasSpecialCharacters;
            hasSpecialCharacters = (System.Text.RegularExpressions.Regex.Matches(value, "[\t\r\v\f\n]").Count != 0);
            return hasSpecialCharacters;
        }
        //Allows only characters in the range of A-Z (case insensitive), 0-9, and “.”, “-“, and “_”.
        public static bool containsNotOnlyAllowedCaracters(string value)
        {
            bool containsNotAllowedCharacters;
            containsNotAllowedCharacters = (System.Text.RegularExpressions.Regex.Matches(value, @"[^0-9a-zA-Z\.\-_]").Count != 0);
            return containsNotAllowedCharacters;
        }

        public static UpdateFieldsModel validateUpdateRecord(string tableName, string columnName, string currentValue, string updatedValue)
        {
            //existingItem.SiteName != model.SiteName
            if (currentValue == updatedValue)
            {
                var updateField = new UpdateFieldsModel(tableName, columnName, currentValue, updatedValue); 
                return updateField;
            }
            else
            {
                return null;
            }           
        }
    }
}
