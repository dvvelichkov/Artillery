using Artillery.Data.Models;
using Artillery.Data.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Artillery.DataProcessor.ImportDto
{
    public class ImportGunDto
    {
        [Required]
        [ForeignKey("Manufacturer")]
        public int ManufacturerId { get; set; }

        [Required]
        [Range(100, 1350000)]
        public int GunWeight { get; set; }

        [Required]
        [Range(2.00, 35.00)]
        public double BarrelLength { get; set; }
        public int? NumberBuild { get; set; }

        [Required]
        [Range(1, 100000)]
        public int Range { get; set; }

        [Required]
        [EnumDataType(typeof(GunType))]
        public string GunType { get; set; }

        [Required]
        [ForeignKey("Shell")]
        public int ShellId { get; set; }

        public ImportJsonCountryDto[] Countries;

    }
}
