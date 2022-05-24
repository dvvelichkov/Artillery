using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Xml.Serialization;

namespace Artillery.DataProcessor.ImportDto
{
    [XmlType("Shells")]
    public class ImportShellDto
    {
        [XmlElement("ShellWeight")]
        [Required]
        //[Range(2.00, 1680.00)]
        public string ShellWeight { get; set; }

        [XmlElement("Caliber")]
        [Required]
        [MaxLength(30)]
        [MinLength(4)]
        public string Caliber { get; set; }
    }
}
