﻿using CsvHelper;
using HydroServerTools.Models;
using HydroserverToolsBusinessObjects;
using HydroserverToolsBusinessObjects.Models;
using HydroServerToolsRepository.Repository;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.ApplicationServer.Caching;
using ProgressReporting.Services;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.UI.WebControls;

namespace HydroServerTools.Controllers.WebApi
{
    public class UploadController : ApiController
    {
        public const string CacheName = "default";
       
        public string instanceIdentifier = HttpContext.Current.User.Identity.Name;



        // Enable both Get and Post so that our jquery call can send data, and get a status
        [HttpGet]
        [HttpPost]
        [Authorize]
        public HttpResponseMessage UploadFile(string id)
        {
            try
            {
                BusinessObjectsUtils.RemoveItemFromCache(instanceIdentifier, Ressources.IMPORT_STATUS_UPLOAD);

                // Get a reference to the file that our jQuery sent.  Even with multiple files, they will all be their own request and be the 0 index
                HttpPostedFile file = HttpContext.Current.Request.Files[0];
                string message = string.Empty;
                if ((file.FileName.ToLower().EndsWith(".csv") || file.FileName.ToLower().EndsWith(".zip")) == false)
                {
                    message = Ressources.FILETYPE_NOT_CSV_ZIP;
                    return new HttpResponseMessage(HttpStatusCode.BadRequest); ;
                }




                var ms = new MemoryStream();
                StreamReader reader = null;
                TextReader textReader = null;
                try
                {
                    if (file.FileName.ToLower().EndsWith(".zip"))
                    {
                        //Updating status
                        //BusinessObjectsUtils.UpdateCachedprocessStatusMessage(instanceIdentifier, CacheName, Ressources.IMPORT_STATUS_EXTRACTNG);

                        BusinessObjectsUtils.RemoveItemFromCache(instanceIdentifier, Ressources.IMPORT_STATUS_EXTRACTNG);

                        using (ZipInputStream zipInputStream = new ZipInputStream(file.InputStream))
                        { 
                            ZipEntry zipEntry = zipInputStream.GetNextEntry();
                            while (zipEntry != null)
                            {
                                String entryFileName = zipEntry.Name;
                                // to remove the folder from the entry:- entryFileName = Path.GetFileName(entryFileName);
                                // Optionally match entrynames against a selection list here to skip as desired.
                                // The unpacked length is available in the zipEntry.Size property.

                                byte[] buffer = new byte[4096];     // 4K is optimum

                                // Manipulate the output filename here as desired.
                                //String fullZipToPath = Path.Combine(outFolder, entryFileName);
                                //string directoryName = Path.GetDirectoryName(fullZipToPath);
                                //if (directoryName.Length > 0)
                                //    Directory.CreateDirectory(directoryName);

                                // Unzip file in buffered chunks. This is just as fast as unpacking to a buffer the full size
                                // of the file, but does not waste memory.
                                // The "using" will close the stream even if an exception occurs.

                                using (MemoryStream streamWriter = new MemoryStream())
                                {
                                    StreamUtils.Copy(zipInputStream, ms, buffer);
                                }



                                //StreamUtils.Copy(zipInputStream, ms, buffer);

                                zipEntry = zipInputStream.GetNextEntry();

                            }
                        }
                        ms.Position = 0;
                        reader = new StreamReader(ms, Encoding.GetEncoding("iso-8859-1"));
                        //ms.Close();
                        /**/
                    }
                    else
                    {
                        BusinessObjectsUtils.RemoveItemFromCache(instanceIdentifier, Ressources.IMPORT_STATUS_PROCESSING);
                        reader = new StreamReader(file.InputStream, Encoding.GetEncoding("iso-8859-1"));
                    }
                    var o = GetDistinct(reader);
                   
                    textReader = new StringReader(o);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    //clean up ressources
                    //writer.Close();
                
                    ms.Close();
                    reader.Close();
                    //textReader.Close();
                    //reader2.Close();
                }


                //int count = 0;
                //if (HydroServerToolsUtils.IsLocalHostServer())
                //{
                //     //clear Session 
                //     var httpContext = (HttpContextWrapper)Request.Properties["MS_HttpContext"];
                //      httpContext.Session.Clear();

                //}
                //else
                // { 
                //clear cache 
                var httpContext = new HttpContextWrapper(System.Web.HttpContext.Current);
                //hack to provide unique id, work around the problem with the session and google ID
                var userid = HttpContext.Current.User.Identity.Name;
                var session = System.Web.HttpContext.Current.Session;
                session["Uploadedfile"] = textReader;


                /*
                BusinessObjectsUtils.RemoveItemFromSession(instanceIdentifier, "listOfCorrectRecords");
                BusinessObjectsUtils.RemoveItemFromSession(instanceIdentifier,  "listOfIncorrectRecords");
                BusinessObjectsUtils.RemoveItemFromSession(instanceIdentifier,  "listOfEditedRecords");
                BusinessObjectsUtils.RemoveItemFromSession(instanceIdentifier,  "listOfDuplicateRecords");
                BusinessObjectsUtils.RemoveItemFromSession(instanceIdentifier,  Ressources.IMPORT_STATUS_UPLOAD);

                 if (file != null)

                     ProcessData(userid, textReader, id, out message);
                else
                    return new HttpResponseMessage(HttpStatusCode.BadRequest);


                if (message.Length > 0) //an error has occured
                {
                    HttpError err = new HttpError(message);
                    return Request.CreateResponse(HttpStatusCode.BadRequest, err);
                }
                
               
                */
                


                //if (message.Length > 0) //an error has occured
                //{
                //    HttpError err = new HttpError(message);
                //    return Request.CreateResponse(HttpStatusCode.BadRequest, err);
                //}
                //if (!ModelState.IsValid)
                //{
                //    var error = ModelState.Values.Any(x => x.Errors.FirstOrDefault());
                //}
                // Now we need to wire up a response so that the calling script understands what happened
                HttpContext.Current.Response.ContentType = "text/plain";
                var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                var result = new { name = file.FileName };

                HttpContext.Current.Response.Write(serializer.Serialize(result));
                HttpContext.Current.Response.StatusCode = 200;

                // For compatibility with IE's "done" event we need to return a result as well as setting the context.response
                return new HttpResponseMessage(HttpStatusCode.OK);    

            }
            
            catch (Exception ex)
            {
                // Now we need to wire up a response so that the calling script understands what happened
                HttpError err = new HttpError(ex.Message);
                return Request.CreateResponse(HttpStatusCode.BadRequest, err);
            }

        }
        [HttpPost]
         public async Task<HttpResponseMessage> startProcess(string id)
        {
            var httpContext = new HttpContextWrapper(System.Web.HttpContext.Current);
            var session = System.Web.HttpContext.Current.Session;
            var userName = HttpContext.Current.User.Identity.Name;
            string message = string.Empty;

            //BusinessObjectsUtils.RemoveItemFromSession(instanceIdentifier, "listOfCorrectRecords");
            //BusinessObjectsUtils.RemoveItemFromSession(instanceIdentifier, "listOfIncorrectRecords");
            //BusinessObjectsUtils.RemoveItemFromSession(instanceIdentifier, "listOfEditedRecords");
            //BusinessObjectsUtils.RemoveItemFromSession(instanceIdentifier, "listOfDuplicateRecords");

            BusinessObjectsUtils.RemoveItemFromCache(instanceIdentifier, "listOfCorrectRecords");
            BusinessObjectsUtils.RemoveItemFromCache(instanceIdentifier, "listOfIncorrectRecords");
            BusinessObjectsUtils.RemoveItemFromCache(instanceIdentifier, "listOfEditedRecords");
            BusinessObjectsUtils.RemoveItemFromCache(instanceIdentifier, "listOfDuplicateRecords");

            BusinessObjectsUtils.RemoveItemFromCache(instanceIdentifier, Ressources.IMPORT_STATUS_UPLOAD);

            if (session["Uploadedfile"] != null)
            {
                var textReader = (TextReader)session["Uploadedfile"];
                BusinessObjectsUtils.UpdateCachedprocessStatusMessage(userName, "Default", Ressources.STATUS_PROCESSING);


                Task.Run(async () =>
                {
                    ProcessData(userName, textReader, id, out message);

                    //int maxn = 10;
                    //for (int n = 0; n > 10; n++)
                    //{

                    //    BusinessObjectsUtils.UpdateCachedprocessStatusMessage(userName, "Default", String.Format(Ressources.IMPORT_STATUS_PROCESSING, n, maxn));
                    //    await Task.Delay(3000);
                    //}
                }).ConfigureAwait(false);
                textReader.Close();
                //await TaskProcessData(userName, textReader, viewName, out message);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            
            //var str = await ProcessData();
            return new HttpResponseMessage(HttpStatusCode.OK); ;
        }


        public HttpResponseMessage startProcess2(string viewName)
        {
            //clear cache 
            var httpContext = new HttpContextWrapper(System.Web.HttpContext.Current);
            var session = System.Web.HttpContext.Current.Session;
            var userName = HttpContext.Current.User.Identity.Name;
            string message = string.Empty;
            if (session["Uploadedfile"] != null)
            {
                var textReader = (TextReader)session["Uploadedfile"];
                ProcessData(userName, textReader, viewName, out message);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            BusinessObjectsUtils.RemoveItemFromSession(instanceIdentifier, "listOfCorrectRecords");
            BusinessObjectsUtils.RemoveItemFromSession(instanceIdentifier, "listOfIncorrectRecords");
            BusinessObjectsUtils.RemoveItemFromSession(instanceIdentifier, "listOfEditedRecords");
            BusinessObjectsUtils.RemoveItemFromSession(instanceIdentifier, "listOfDuplicateRecords");
            BusinessObjectsUtils.RemoveItemFromSession(instanceIdentifier, Ressources.IMPORT_STATUS_UPLOAD);

           


            if (message.Length > 0) //an error has occured
            {
                HttpError err = new HttpError(message);
                return Request.CreateResponse(HttpStatusCode.BadRequest, err);
            }
            else
            {
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
        }

        private void ProcessData(string username, TextReader file, string viewName, out string message)
        {

            //string viewName = "sites";
            string entityConnectionString = string.Empty;
            message = string.Empty;
            
            //Get Connection string
            if (!String.IsNullOrEmpty(username))
            {
                entityConnectionString = HydroServerToolsUtils.BuildConnectionStringForUserName(username);

                if (String.IsNullOrEmpty(entityConnectionString))
                {
                    //entityConnectionString = HydroServerToolsUtils.GetDBEntityConnectionStringByName(connectionName);
                    //if (string.IsNullOrEmpty(entityConnectionString))
                    //{
                    message = Ressources.HYDROSERVER_USERLOOKUP_FAILED;
                    return;
                    //}
                }
                //else
                //{
                //    message = Ressources.HYDROSERVER_USERLOOKUP_FAILED;

                //    return;

                //}
            }
            else
            {
                message = Ressources.HYDROSERVER_USERLOOKUP_FAILED;
                return;

                //entityConnectionString = Utils.GetDBConnectionStringByName("Hydroservertest2");

            }



            //Object T;
            try
            {
                #region Sites
                //  
                if (viewName.ToLower() == "sites")
                {
                    List<SiteModel> values = null;


                    // var siteViewModel = new SitesViewModel();
                    // Type t = typeof(SiteModel);

                    var listOfIncorrectRecords = new List<SiteModel>();
                    var listOfCorrectRecords = new List<SiteModel>();
                    var listOfDuplicateRecords = new List<SiteModel>();
                    var listOfEditedRecords = new List<SiteModel>();

                    // Verify that the user selected a file
                    //if (file != null && file.ContentLength > 0)
                    //{

                        values = parseCSV<SiteModel>(file, viewName);
                    //}


                    if (values != null)
                    {
                        if (values.Count > 0)
                        {
                            var repository = new SitesRepository();

                            repository.AddSites(values, entityConnectionString, instanceIdentifier, out listOfIncorrectRecords, out listOfCorrectRecords, out listOfDuplicateRecords, out listOfEditedRecords);

                            //PutRecordsInSession<SiteModel>(listOfIncorrectRecords, listOfCorrectRecords, listOfDuplicateRecords, listOfEditedRecords);
                            PutRecordsInCache<SiteModel>(listOfIncorrectRecords, listOfCorrectRecords, listOfDuplicateRecords, listOfEditedRecords, instanceIdentifier);
                        }
                        else
                        {

                            //throw new ArgumentException(String.Format(Ressources.IMPORT_FAILED_NOVALIDDATA, file.FileName));

                        }
                    }



                }
                #endregion
                #region Variables
                if (viewName.ToLower() == "variables")
                {
                    List<VariablesModel> values = null;


                    // var siteViewModel = new SitesViewModel();
                    // Type t = typeof(SiteModel);

                    var listOfIncorrectRecords = new List<VariablesModel>();
                    var listOfCorrectRecords = new List<VariablesModel>();
                    var listOfDuplicateRecords = new List<VariablesModel>();
                    var listOfEditedRecords = new List<VariablesModel>();


                    // Verify that the user selected a file
                    //if (file != null && file.ContentLength > 0)
                    //{

                        values = parseCSV<VariablesModel>(file, viewName);
                    //}


                    if (values != null)
                    {
                        var repository = new VariablesRepository();

                        repository.AddVariables(values, entityConnectionString, instanceIdentifier, out listOfIncorrectRecords, out listOfCorrectRecords, out listOfDuplicateRecords, out listOfEditedRecords);

                    }

                    //PutRecordsInSession<VariablesModel>(listOfIncorrectRecords, listOfCorrectRecords, listOfDuplicateRecords, listOfEditedRecords);
                    PutRecordsInCache<VariablesModel>(listOfIncorrectRecords, listOfCorrectRecords, listOfDuplicateRecords, listOfEditedRecords, instanceIdentifier);

                }
                #endregion
                #region OffsetTypes
                if (viewName.ToLower() == "offsettypes")
                {
                    List<OffsetTypesModel> values = null;


                    // var siteViewModel = new SitesViewModel();
                    // Type t = typeof(SiteModel);

                    var listOfIncorrectRecords = new List<OffsetTypesModel>();
                    var listOfCorrectRecords = new List<OffsetTypesModel>();
                    var listOfDuplicateRecords = new List<OffsetTypesModel>();
                    var listOfEditedRecords = new List<OffsetTypesModel>();


                    // Verify that the user selected a file
                    //if (file != null && file.ContentLength > 0)
                    //{

                        values = parseCSV<OffsetTypesModel>(file, viewName);
                    //}


                    if (values != null)
                    {
                        var repository = new OffsetTypesRepository();

                        repository.AddOffsetTypes(values, entityConnectionString, instanceIdentifier, out listOfIncorrectRecords, out listOfCorrectRecords, out listOfDuplicateRecords, out listOfEditedRecords);
                    }

                    //PutRecordsInSession<OffsetTypesModel>(listOfIncorrectRecords, listOfCorrectRecords, listOfDuplicateRecords, listOfEditedRecords);

                    PutRecordsInCache<OffsetTypesModel>(listOfIncorrectRecords, listOfCorrectRecords, listOfDuplicateRecords, listOfEditedRecords, instanceIdentifier);
                }
                #endregion
                #region Sources
                if (viewName.ToLower() == "sources")
                {
                    List<SourcesModel> values = null;


                    // var siteViewModel = new SitesViewModel();
                    // Type t = typeof(SiteModel);

                    var listOfIncorrectRecords = new List<SourcesModel>();
                    var listOfCorrectRecords = new List<SourcesModel>();
                    var listOfDuplicateRecords = new List<SourcesModel>();
                    var listOfEditedRecords = new List<SourcesModel>();


                    // Verify that the user selected a file
                    //if (file != null && file.ContentLength > 0)
                    //{

                        values = parseCSV<SourcesModel>(file, viewName);
                    //}


                    if (values != null)
                    {
                        var repository = new SourcesRepository();

                        repository.AddSources(values, entityConnectionString, instanceIdentifier, out listOfIncorrectRecords, out listOfCorrectRecords, out listOfDuplicateRecords, out listOfEditedRecords);
                    }

                    //PutRecordsInSession<SourcesModel>(listOfIncorrectRecords, listOfCorrectRecords, listOfDuplicateRecords, listOfEditedRecords);
                    PutRecordsInCache<SourcesModel>(listOfIncorrectRecords, listOfCorrectRecords, listOfDuplicateRecords, listOfEditedRecords, instanceIdentifier);

                }
                #endregion
                #region Methods
                if (viewName.ToLower() == "methods")
                {
                    List<MethodModel> values = null;


                    // var siteViewModel = new SitesViewModel();
                    // Type t = typeof(SiteModel);

                    var listOfIncorrectRecords = new List<MethodModel>();
                    var listOfCorrectRecords = new List<MethodModel>();
                    var listOfDuplicateRecords = new List<MethodModel>();
                    var listOfEditedRecords = new List<MethodModel>();


                    // Verify that the user selected a file
                    //if (file != null && file.ContentLength > 0)
                    //{

                        values = parseCSV<MethodModel>(file, viewName);
                    //}


                    if (values != null)
                    {
                        var repository = new MethodsRepository();

                        repository.AddMethods(values, entityConnectionString, instanceIdentifier, out listOfIncorrectRecords, out listOfCorrectRecords, out listOfDuplicateRecords, out listOfEditedRecords);
                    }

                    //PutRecordsInSession<MethodModel>(listOfIncorrectRecords, listOfCorrectRecords, listOfDuplicateRecords, listOfEditedRecords);
                    PutRecordsInCache<MethodModel>(listOfIncorrectRecords, listOfCorrectRecords, listOfDuplicateRecords, listOfEditedRecords, instanceIdentifier);

                }
                #endregion
                #region LabMethods
                if (viewName.ToLower() == "labmethods")
                {
                    List<LabMethodModel> values = null;


                    // var siteViewModel = new SitesViewModel();
                    // Type t = typeof(SiteModel);

                    var listOfIncorrectRecords = new List<LabMethodModel>();
                    var listOfCorrectRecords = new List<LabMethodModel>();
                    var listOfDuplicateRecords = new List<LabMethodModel>();
                    var listOfEditedRecords = new List<LabMethodModel>();


                    // Verify that the user selected a file
                    //if (file != null && file.ContentLength > 0)
                    //{

                        values = parseCSV<LabMethodModel>(file, viewName);
                    //}


                    if (values != null)
                    {
                        var repository = new LabMethodsRepository();

                        repository.AddLabMethods(values, entityConnectionString, instanceIdentifier, out listOfIncorrectRecords, out listOfCorrectRecords, out listOfDuplicateRecords, out listOfEditedRecords);
                    }

                    //PutRecordsInSession<LabMethodModel>(listOfIncorrectRecords, listOfCorrectRecords, listOfDuplicateRecords, listOfEditedRecords);
                    PutRecordsInCache<LabMethodModel>(listOfIncorrectRecords, listOfCorrectRecords, listOfDuplicateRecords, listOfEditedRecords, instanceIdentifier);

                }
                #endregion
                #region Samples
                if (viewName.ToLower() == "samples")
                {
                    List<SampleModel> values = null;


                    // var siteViewModel = new SitesViewModel();
                    // Type t = typeof(SiteModel);

                    var listOfIncorrectRecords = new List<SampleModel>();
                    var listOfCorrectRecords = new List<SampleModel>();
                    var listOfDuplicateRecords = new List<SampleModel>();
                    var listOfEditedRecords = new List<SampleModel>();


                    // Verify that the user selected a file
                    //if (file != null && file.ContentLength > 0)
                    //{

                        values = parseCSV<SampleModel>(file, viewName);
                    //}


                    if (values != null)
                    {
                        var repository = new SamplesRepository();

                        repository.AddSamples(values, entityConnectionString, instanceIdentifier, out listOfIncorrectRecords, out listOfCorrectRecords, out listOfDuplicateRecords, out listOfEditedRecords);
                    }

                    //PutRecordsInSession<SampleModel>(listOfIncorrectRecords, listOfCorrectRecords, listOfDuplicateRecords, listOfEditedRecords);
                    PutRecordsInCache<SampleModel>(listOfIncorrectRecords, listOfCorrectRecords, listOfDuplicateRecords, listOfEditedRecords, instanceIdentifier);

                }
                #endregion
                #region Qualifiers
                if (viewName.ToLower() == "qualifiers")
                {
                    List<QualifiersModel> values = null;


                    // var siteViewModel = new SitesViewModel();
                    // Type t = typeof(SiteModel);

                    var listOfIncorrectRecords = new List<QualifiersModel>();
                    var listOfCorrectRecords = new List<QualifiersModel>();
                    var listOfDuplicateRecords = new List<QualifiersModel>();
                    var listOfEditedRecords = new List<QualifiersModel>();


                    // Verify that the user selected a file
                    //if (file != null && file.ContentLength > 0)
                    //{

                        values = parseCSV<QualifiersModel>(file, viewName);
                    //}


                    if (values != null)
                    {
                        var repository = new QualifiersRepository();

                        repository.AddQualifiers(values, entityConnectionString, instanceIdentifier, out listOfIncorrectRecords, out listOfCorrectRecords, out listOfDuplicateRecords, out listOfEditedRecords);
                    }

                    //PutRecordsInSession<QualifiersModel>(listOfIncorrectRecords, listOfCorrectRecords, listOfDuplicateRecords, listOfEditedRecords);
                    PutRecordsInCache<QualifiersModel>(listOfIncorrectRecords, listOfCorrectRecords, listOfDuplicateRecords, listOfEditedRecords, instanceIdentifier);

                }
                #endregion
                #region QualityControlLevels
                if (viewName.ToLower() == "qualitycontrollevels")
                {
                    List<QualityControlLevelModel> values = null;


                    // var siteViewModel = new SitesViewModel();
                    // Type t = typeof(SiteModel);

                    var listOfIncorrectRecords = new List<QualityControlLevelModel>();
                    var listOfCorrectRecords = new List<QualityControlLevelModel>();
                    var listOfDuplicateRecords = new List<QualityControlLevelModel>();
                    var listOfEditedRecords = new List<QualityControlLevelModel>();


                    // Verify that the user selected a file
                    //if (file != null && file.ContentLength > 0)
                    //{

                        values = parseCSV<QualityControlLevelModel>(file, viewName);
                    //}


                    if (values != null)
                    {
                        var repository = new QualityControlLevelsRepository();

                        repository.AddQualityControlLevel(values, entityConnectionString, instanceIdentifier, out listOfIncorrectRecords, out listOfCorrectRecords, out listOfDuplicateRecords, out listOfEditedRecords);
                    }

                    //PutRecordsInSession<QualityControlLevelModel>(listOfIncorrectRecords, listOfCorrectRecords, listOfDuplicateRecords, listOfEditedRecords);
                    PutRecordsInCache<QualityControlLevelModel>(listOfIncorrectRecords, listOfCorrectRecords, listOfDuplicateRecords, listOfEditedRecords, instanceIdentifier);

                }
                #endregion
                #region Datavalues
                //  
                if (viewName.ToLower() == "datavalues")
                {
                    List<DataValuesModel> values = null;


                    //var siteViewModel = new SitesViewModel();
                    var listOfIncorrectRecords = new List<DataValuesModel>();
                    var listOfCorrectRecords = new List<DataValuesModel>();
                    var listOfDuplicateRecords = new List<DataValuesModel>();
                    var listOfEditedRecords = new List<DataValuesModel>();

                    
                    // Verify that the user selected a file
                    //if (file != null && file.ContentLength > 0)
                    //{
                        
                        values = parseCSV<DataValuesModel>(file, viewName);
                    //}


                    if (values != null)
                    {
                        var repository = new DataValuesRepository();

                        repository.AddDataValues(values, entityConnectionString, instanceIdentifier, out listOfIncorrectRecords, out listOfCorrectRecords, out listOfDuplicateRecords, out listOfEditedRecords);
                        ////update Seriescatalog
                        //if (listOfCorrectRecords.Count() > 0)
                        //{
                        //    var seriesCatalogRepository = new SeriesCatalogRepository();
                        //    seriesCatalogRepository.deleteAll(entityConnectionString);
                        //    seriesCatalogRepository.UpdateSeriesCatalog(MvcApplication.InstanceGuid, entityConnectionString);
                        //}
                    }

                    PutRecordsInCache<DataValuesModel>(listOfIncorrectRecords, listOfCorrectRecords, listOfDuplicateRecords, listOfEditedRecords, instanceIdentifier);

                }
                #endregion
                #region GroupDescriptions
                if (viewName.ToLower() == "groupdescriptions")
                {
                    List<GroupDescriptionModel> values = null;


                    // var siteViewModel = new SitesViewModel();
                    // Type t = typeof(SiteModel);

                    var listOfIncorrectRecords = new List<GroupDescriptionModel>();
                    var listOfCorrectRecords = new List<GroupDescriptionModel>();
                    var listOfDuplicateRecords = new List<GroupDescriptionModel>();
                    var listOfEditedRecords = new List<GroupDescriptionModel>();


                    // Verify that the user selected a file
                    //if (file != null && file.ContentLength > 0)
                    //{

                        values = parseCSV<GroupDescriptionModel>(file, viewName);
                    //}


                    if (values != null)
                    {
                        var repository = new GroupDescriptionsRepository();

                        repository.AddGroupDescriptions(values, entityConnectionString, instanceIdentifier, out listOfIncorrectRecords, out listOfCorrectRecords, out listOfDuplicateRecords, out listOfEditedRecords);
                    }

                       PutRecordsInCache<GroupDescriptionModel>(listOfIncorrectRecords, listOfCorrectRecords, listOfDuplicateRecords, listOfEditedRecords, instanceIdentifier);


                }
                #endregion
                #region Groups
                if (viewName.ToLower() == "groups")
                {
                    List<GroupsModel> values = null;


                    // var siteViewModel = new SitesViewModel();
                    // Type t = typeof(SiteModel);

                    var listOfIncorrectRecords = new List<GroupsModel>();
                    var listOfCorrectRecords = new List<GroupsModel>();
                    var listOfDuplicateRecords = new List<GroupsModel>();
                    var listOfEditedRecords = new List<GroupsModel>();

                    // Verify that the user selected a file
                    //if (file != null && file.ContentLength > 0)
                    //{

                        values = parseCSV<GroupsModel>(file, viewName);
                    //}


                    if (values != null)
                    {
                        var repository = new GroupsRepository();

                        repository.AddGroups(values, entityConnectionString, instanceIdentifier, out listOfIncorrectRecords, out listOfCorrectRecords, out listOfDuplicateRecords, out listOfEditedRecords);
                    }

                    //PutRecordsInSession<GroupsModel>(listOfIncorrectRecords, listOfCorrectRecords, listOfDuplicateRecords, listOfEditedRecords);
                    PutRecordsInCache<GroupsModel>(listOfIncorrectRecords, listOfCorrectRecords, listOfDuplicateRecords, listOfEditedRecords, instanceIdentifier);


                }
                #endregion
                #region DerivedFrom
                if (viewName.ToLower() == "derivedfrom")
                {
                    List<DerivedFromModel> values = null;


                    // var siteViewModel = new SitesViewModel();
                    // Type t = typeof(SiteModel);

                    var listOfIncorrectRecords = new List<DerivedFromModel>();
                    var listOfCorrectRecords = new List<DerivedFromModel>();
                    var listOfDuplicateRecords = new List<DerivedFromModel>();
                    var listOfEditedRecords = new List<DerivedFromModel>();


                    // Verify that the user selected a file
                    //if (file != null && file.ContentLength > 0)
                    //{

                        values = parseCSV<DerivedFromModel>(file, viewName);
                    //}


                    if (values != null)
                    {
                        var repository = new DerivedFromRepository();

                        repository.AddDerivedFrom(values, entityConnectionString, instanceIdentifier, out listOfIncorrectRecords, out listOfCorrectRecords, out listOfDuplicateRecords, out listOfEditedRecords);
                    }

                    //PutRecordsInSession<DerivedFromModel>(listOfIncorrectRecords, listOfCorrectRecords, listOfDuplicateRecords, listOfEditedRecords);
                    PutRecordsInCache<DerivedFromModel>(listOfIncorrectRecords, listOfCorrectRecords, listOfDuplicateRecords, listOfEditedRecords, instanceIdentifier);

                }
                #endregion
                #region Categories
                if (viewName.ToLower() == "categories")
                {
                    List<CategoriesModel> values = null;


                    // var siteViewModel = new SitesViewModel();
                    // Type t = typeof(SiteModel);

                    var listOfIncorrectRecords = new List<CategoriesModel>();
                    var listOfCorrectRecords = new List<CategoriesModel>();
                    var listOfDuplicateRecords = new List<CategoriesModel>();
                    var listOfEditedRecords = new List<CategoriesModel>();


                    // Verify that the user selected a file
                    //if (file != null && file.ContentLength > 0)
                    //{

                        values = parseCSV<CategoriesModel>(file, viewName);
                    //}


                    if (values != null)
                    {
                        if (values.Count() == 0) return;

                        var repository = new CategoriesRepository();

                        repository.AddCategories(values, entityConnectionString, instanceIdentifier, out listOfIncorrectRecords, out listOfCorrectRecords, out listOfDuplicateRecords, out listOfEditedRecords);
                    }

                    //PutRecordsInSession<CategoriesModel>(listOfIncorrectRecords, listOfCorrectRecords, listOfDuplicateRecords, listOfEditedRecords);
                    PutRecordsInCache<CategoriesModel>(listOfIncorrectRecords, listOfCorrectRecords, listOfDuplicateRecords, listOfEditedRecords, instanceIdentifier);

                }
                //setting process to complete IMPORTANT to trigger redirect
                BusinessObjectsUtils.UpdateCachedprocessStatusMessage(instanceIdentifier, CacheName, String.Format(Ressources.IMPORT_STATUS_PROCESSING_DONE));

                #endregion
            }
            catch (Exception ex)
            {
                BusinessObjectsUtils.UpdateCachedprocessStatusMessage(instanceIdentifier, CacheName, ex.Message.ToString());

                throw;
            }
            return;
        }

        private static void PutRecordsInCache<T>(List<T> listOfIncorrectRecords, List<T> listOfCorrectRecords, List<T> listOfDuplicateRecords, List<T> listOfEditedRecords, string identifier)
            {
                //if (HydroServerToolsUtils.IsLocalHostServer())
                //{
                //    //var httpContext = (HttpContextWrapper)Request.Properties["MS_HttpContext"];


                //    var httpContext = new HttpContextWrapper(HttpContext.Current);

                //    if (httpContext.Session["listOfCorrectRecords"] == null) httpContext.Session["listOfCorrectRecords"] = listOfCorrectRecords; else httpContext.Session["listOfCorrectRecords"] = listOfCorrectRecords;
                //    if (httpContext.Session["listOfIncorrectRecords"] == null) httpContext.Session["listOfIncorrectRecords"] = listOfIncorrectRecords; else httpContext.Session["listOfIncorrectRecords"] = listOfIncorrectRecords;
                //    if (httpContext.Session["listOfEditedRecords"] == null) httpContext.Session["listOfEditedRecords"] = listOfEditedRecords; else httpContext.Session["listOfEditedRecords"] = listOfEditedRecords;
                //    if (httpContext.Session["listOfDuplicateRecords"] == null) httpContext.Session["listOfDuplicateRecords"] = listOfDuplicateRecords; else httpContext.Session["listOfDuplicateRecords"] = listOfDuplicateRecords;

                //}
                //else
                //{
                    var cache = HttpRuntime.Cache;
                    

                    if (HttpRuntime.Cache.Get(identifier + "listOfCorrectRecords") == null) HttpRuntime.Cache.Insert(identifier + "listOfCorrectRecords", listOfCorrectRecords); else HttpRuntime.Cache[identifier + "listOfCorrectRecords"] = listOfCorrectRecords;
                    if (HttpRuntime.Cache.Get(identifier + "listOfIncorrectRecords") == null) HttpRuntime.Cache.Insert(identifier + "listOfIncorrectRecords", listOfIncorrectRecords); else HttpRuntime.Cache[identifier + "listOfIncorrectRecords"] = listOfIncorrectRecords;
                    if (HttpRuntime.Cache.Get(identifier + "listOfEditedRecords") == null) HttpRuntime.Cache.Insert(identifier + "listOfEditedRecords", listOfEditedRecords); else HttpRuntime.Cache[identifier + "listOfEditedRecords"] = listOfEditedRecords;
                    if (HttpRuntime.Cache.Get(identifier + "listOfDuplicateRecords") == null) HttpRuntime.Cache.Insert(identifier + "listOfDuplicateRecords", listOfDuplicateRecords); else HttpRuntime.Cache[identifier + "listOfDuplicateRecords"] = listOfDuplicateRecords;


                    //if (cache.Get(identifier + "listOfCorrectRecords") == null) cache.Insert(identifier + "listOfCorrectRecords", listOfCorrectRecords); else cache.(identifier + "listOfCorrectRecords", listOfCorrectRecords);
                    //if (cache.Get(identifier + "listOfIncorrectRecords") == null) cache.Insert(identifier + "listOfIncorrectRecords", listOfIncorrectRecords); else cache.Put(identifier + "listOfIncorrectRecords", listOfIncorrectRecords);
                    //if (cache.Get(identifier + "listOfEditedRecords") == null) cache.Insert(identifier + "listOfEditedRecords", listOfEditedRecords); else cache.Put(identifier + "listOfEditedRecords", listOfEditedRecords);
                    //if (cache.Get(identifier + "listOfDuplicateRecords") == null) cache.Insert(identifier + "listOfDuplicateRecords", listOfDuplicateRecords); else cache.Put(identifier + "listOfDuplicateRecords", listOfDuplicateRecords);

                //}
            }

        private static void PutRecordsInSession<T>(List<T> listOfIncorrectRecords, List<T> listOfCorrectRecords, List<T> listOfDuplicateRecords, List<T> listOfEditedRecords)
            {
                //if (HydroServerToolsUtils.IsLocalHostServer())
                //{
                //    //var httpContext = (HttpContextWrapper)Request.Properties["MS_HttpContext"];


                var session = System.Web.HttpContext.Current.Session;

                if (session["listOfCorrectRecords"] == null) session["listOfCorrectRecords"] = listOfCorrectRecords; else session["listOfCorrectRecords"] = listOfCorrectRecords;
                if (session["listOfIncorrectRecords"] == null) session["listOfIncorrectRecords"] = listOfIncorrectRecords; else session["listOfIncorrectRecords"] = listOfIncorrectRecords;
                if (session["listOfEditedRecords"] == null) session["listOfEditedRecords"] = listOfEditedRecords; else session["listOfEditedRecords"] = listOfEditedRecords;
                if (session["listOfDuplicateRecords"] == null) session["listOfDuplicateRecords"] = listOfDuplicateRecords; else session["listOfDuplicateRecords"] = listOfDuplicateRecords;

                //}
                //else
                //{
                //DataCache cache = new DataCache("default");
                //var identifier = MvcApplication.InstanceGuid + System.Web.HttpContext.Current.User.Identity.Name;
                //if (cache.Get(identifier + "listOfCorrectRecords") == null) cache.Add(identifier + "listOfCorrectRecords", listOfCorrectRecords); else cache.Put(identifier + "listOfCorrectRecords", listOfCorrectRecords);
                //if (cache.Get(identifier + "listOfIncorrectRecords") == null) cache.Add(identifier + "listOfIncorrectRecords", listOfIncorrectRecords); else cache.Put(identifier + "listOfIncorrectRecords", listOfIncorrectRecords);
                //if (cache.Get(identifier + "listOfEditedRecords") == null) cache.Add(identifier + "listOfEditedRecords", listOfEditedRecords); else cache.Put(identifier + "listOfEditedRecords", listOfEditedRecords);
                //if (cache.Get(identifier + "listOfDuplicateRecords") == null) cache.Add(identifier + "listOfDuplicateRecords", listOfDuplicateRecords); else cache.Put(identifier + "listOfDuplicateRecords", listOfDuplicateRecords);

                //}
            }
        
        private static StreamReader getStreamfromZip(HttpPostedFile file)
        {
            StreamReader reader = null;
            using (ZipArchive archive = new ZipArchive(file.InputStream))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    var stream = entry.Open();

                      reader = new StreamReader(stream, Encoding.GetEncoding("iso-8859-1"));

                    //Do awesome stream stuff!!
                }
            }
            return reader;
        }

        private static List<T> parseCSV<T>(TextReader textReader, string viewName)
        {
            var s = new List<T>();
            
            //var outFolder = "";

            string instanceIdentifier = "";// MvcApplication.InstanceGuid + HttpContext.Current.User.Identity.Name;

            
            //remove illegal characters
            //MemoryStream output = new MemoryStream();
            //var writer = new StreamWriter(output, Encoding.GetEncoding("iso-8859-1"));
            //while (!reader.EndOfStream)
            //{
            //    var currentLine = reader.ReadLine();
            //    currentLine = HydroServerToolsUtils.stripNonValidXMLCharacters(currentLine);
            //    writer.WriteLine(currentLine);

            //}
            //writer.Flush();
            //output.Position = 0;
            //var reader2 = new StreamReader(output, Encoding.GetEncoding("iso-8859-1"));

           
            //using (TextReader sr = new StringReader(o))
            //{
            //    DoSomethingWithATextReader(sr);
            //}
            var csvReader = new CsvHelper.CsvReader(textReader);


            //while (csvReader.Read())
            //{
            //var intField = csvReader.GetField<int>(0);
            csvReader.Configuration.IsHeaderCaseSensitive = false;
            csvReader.Configuration.WillThrowOnMissingField = false;
            csvReader.Configuration.SkipEmptyRecords = true;

            //while (csvReader.Read())
            //{

            //    break;
            //}

            try
            {
                s = csvReader.GetRecords<T>().ToList();

                var missingMandatoryFields = HydroServerToolsUtils.ValidateFields<T>(csvReader.FieldHeaders.ToList());
                if (missingMandatoryFields.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var item in missingMandatoryFields)
                    {
                        sb.Append(item);
                        sb.Append(",");
                    }
                    var f = sb.ToString().TrimEnd(',');
                    throw new System.ArgumentException(String.Format(Ressources.IMPORT_FAILED_MISSINGMANDATORYFIELDS, f));
                }

            }
            catch (CsvMissingFieldException ex)
            {
                throw;
            }
            catch (CsvReaderException ex)
            {
                throw new System.ArgumentException(String.Format(Ressources.IMPORT_FAILED_NOMATCHINGFIELDS));
            }
            catch (Exception ex)
            {
                throw;
            }
            


            return s;
        }

        static string GetDistinct(StreamReader reader)
        {
            Stopwatch sw = new Stopwatch();//just a timer
            //var s = new StreamReader();
             int maxAllowedRows = int.Parse(System.Configuration.ConfigurationManager.AppSettings["maxAllowedRows"]);

        List<HashSet<string>> lines = new List<HashSet<string>>(); //Hashset is very fast in searching duplicates
            HashSet<string> current = new HashSet<string>(); //This hashset is used at the moment
            lines.Add(current); //Add the current Hashset to a list of hashsets
            sw.Restart(); //just a timer
            Console.WriteLine("Reading File"); //just an output message
            //foreach (string line in reader.ReadLine)
            string line;
            var sb = new StringBuilder();
            while (( line = reader.ReadLine()) != null)

            {
                try
                {
                    if (lines.TrueForAll(hSet => !hSet.Contains(line))) //Look for an existing entry in one of the hashsets
                    {
                        current.Add(line); //If line not found, at the line to the current hashset
                        sb.Append(line); //Fill the list of strings
                        sb.AppendLine();
                        if (lines.Count() > maxAllowedRows) throw new System.OperationCanceledException("Upload exceeds max allowed rows (" + maxAllowedRows + ") per upload");
                        //Debug.WriteLine(current.Count());
                    }
                }
                catch (OutOfMemoryException ex) //Hashset throws an Exception by ca 12,000,000 elements
                {
                    current = new HashSet<string>() { line }; //The line could not added before, add the line to the new hashset
                    lines.Add(current); //add the current hashset to the List of hashsets
                }
            }
            sw.Stop();//just a timer
            Console.WriteLine("File distinct read in " + sw.Elapsed.TotalMilliseconds + " ms");//just an output message
            
            
            //List<string> concatinated = new List<string>(); //Create a list of strings out of the hashset list
            //lines.ForEach(set => concatinated.AddRange(set)); //Fill the list of strings
            //current.ForEach(set =>  sb.Append(lines)); //Fill the list of strings
            
            return sb.ToString(); //Return the list
        }
        [HttpGet]
        [HttpPost]
        public HttpResponseMessage Progress()
        {
            //DataCache cache = new DataCache("default");
            var identifier = User.Identity.Name;
            var StatusMessage = string.Empty;
            //var session = Request.RequestContext.HttpContext.Session;
            if (HttpRuntime.Cache.Get(identifier + "_processStatus") != null)
            {
                StatusMessage = HttpRuntime.Cache.Get(identifier + "_processStatus").ToString();
            }


            //if (session != null)
            //{
            //    if (session["processStatus"] != null)
            //    {
            //        StatusMessage = (string)session["processStatus"];
            //        //StatusMessage = "in proc";
            //    }
            //}
            //return Json(StatusMessage).ToString();
            HttpContext.Current.Response.ContentType = "text/plain";
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            var result = new { name = "test" };

            HttpContext.Current.Response.Write(serializer.Serialize(result));
            HttpContext.Current.Response.StatusCode = 200;

            // For compatibility with IE's "done" event we need to return a result as well as setting the context.response
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpPost]
        public void UploadFile()
        {
            if (HttpContext.Current.Request.Files.AllKeys.Any())
            {
                // Get the uploaded image from the Files collection
                var httpPostedFile = HttpContext.Current.Request.Files["UploadedImage"];

                if (httpPostedFile != null)
                {
                    // Validate the uploaded image(optional)

                    // Get the complete file path
                    var fileSavePath = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedFiles"), httpPostedFile.FileName);

                    // Save the uploaded file to "UploadedFiles" folder
                    httpPostedFile.SaveAs(fileSavePath);
                }
            }
        }

    }
}
