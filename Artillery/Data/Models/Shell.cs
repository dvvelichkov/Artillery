﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Artillery.Data.Models
{
    public class Shell
    {
        public Shell()
        {
            this.Guns = new HashSet<Gun>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        [Range(2.00, 1680.00)]
        public double ShellWeight { get; set; }

        [Required]
        [MaxLength(30)]
        //MinLength in the DTO
        public string Caliber { get; set; }
        public virtual ICollection<Gun> Guns { get; set; }
    }
}

//•	Id – integer, Primary Key
//•	ShellWeight – double in range  [2…1_680] (required)
//•	Caliber – text with length [4…30] (required)
//•	Guns – a collection of Gun

