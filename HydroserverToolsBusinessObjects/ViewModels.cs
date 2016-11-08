﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace HydroServerTools.Models
{
    
        public class DataValuesViewModel
        {
            [Display(Name = "NotVisible")]
            public string ValueID { get; set; }
            [Required]
            public string DataValue { get; set; }

            public string ValueAccuracy { get; set; }
            [Required]
            public string LocalDateTime { get; set; }
            [Required]
            public string UTCOffset { get; set; }
            public string DateTimeUTC { get; set; }

            [Display(Name = "NotVisible")]//
            public string SiteID { get; set; }
            [Required]
            public string SiteCode { get; set; }

            [Display(Name = "NotVisible")]//
            public string VariableID { get; set; }
            [Required]
            public string VariableCode { get; set; }

            public string OffsetValue { get; set; }

            [Display(Name = "NotVisible")]//
            public string OffsetTypeID { get; set; }
            public string OffsetTypeCode { get; set; }

            [Required]
            public string CensorCode { get; set; }

            [Display(Name = "NotVisible")]//
            public string QualifierID { get; set; }
            public string QualifierCode { get; set; }

            [Display(Name = "NotVisible")]//
            public string MethodID { get; set; }
            [Required]
            public string MethodCode { get; set; }
            public string MethodDescription { get; set; }

            [Required]
            public string SourceCode { get; set; }
            [Display(Name = "NotVisible")]//
            public string SourceID { get; set; }

            [Display(Name = "NotVisible")]//
            public string SampleID { get; set; }
            public string LabSampleCode { get; set; }
            public string DerivedFromID { get; set; }
            [Required]
            public string QualityControlLevelCode { get; set; }
            [Display(Name = "NotVisible")]//
            public string QualityControlLevelID { get; set; }

            public string Errors { get; set; }
        }
    }