namespace Artillery.DataProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using Artillery.Data;
    using Artillery.Data.Models;
    using Artillery.Data.Models.Enums;
    using Artillery.DataProcessor.ImportDto;
    using Newtonsoft.Json;

    public class Deserializer
    {
        private const string ErrorMessage =
                "Invalid data.";
        private const string SuccessfulImportCountry =
            "Successfully import {0} with {1} army personnel.";
        private const string SuccessfulImportManufacturer =
            "Successfully import manufacturer {0} founded in{1},{2}.";
        private const string SuccessfulImportShell =
            "Successfully import shell caliber #{0} weight {1} kg.";
        private const string SuccessfulImportGun =
            "Successfully import gun {0} with a total weight of {1} kg. and barrel length of {2} m.";

        public static string ImportCountries(ArtilleryContext context, string xmlString)
        {
            StringBuilder sb = new StringBuilder();
            XmlRootAttribute xmlRoot = new XmlRootAttribute("Countries");
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ImportCountryDto[]), xmlRoot);

            using StringReader stringReader = new StringReader(xmlString);
            ImportCountryDto[] countryDtos = (ImportCountryDto[])xmlSerializer.Deserialize(stringReader);

            HashSet<Country> validCountries = new HashSet<Country>();

            foreach (ImportCountryDto countryDto in countryDtos)
            {
                if (countryDto.CountryName.Length < 4 || countryDto.CountryName.Length > 60
                    || countryDto.ArmySize < 50000 || countryDto.ArmySize > 10000000)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }
                if (string.IsNullOrEmpty(countryDto.CountryName))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }
                Country country = new Country
                {
                    CountryName = countryDto.CountryName,
                    ArmySize = countryDto.ArmySize
                };
                sb.AppendLine(string.Format(SuccessfulImportCountry, country.CountryName, country.ArmySize));

                validCountries.Add(country);
            }
            context.Countries.AddRange(validCountries);
            context.SaveChanges();
            return sb.ToString().TrimEnd();

        }

        public static string ImportManufacturers(ArtilleryContext context, string xmlString)
        {
            StringBuilder sb = new StringBuilder();
            XmlRootAttribute xmlRoot = new XmlRootAttribute("Manufacturers");
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ImportManufacturerDto[]), xmlRoot);

            using StringReader stringReader = new StringReader(xmlString);
            ImportManufacturerDto[] manufacturerDtos = (ImportManufacturerDto[])xmlSerializer.Deserialize(stringReader);

            HashSet<Manufacturer> validManufacturers = new HashSet<Manufacturer>();

            foreach (ImportManufacturerDto manufacturerDto in manufacturerDtos)
            {
                if (string.IsNullOrEmpty(manufacturerDto.ManufacturerName) || string.IsNullOrEmpty(manufacturerDto.Founded)
                    || manufacturerDto.ManufacturerName.Length < 4 || manufacturerDto.ManufacturerName.Length > 40
                    || manufacturerDto.Founded.Length < 10 || manufacturerDto.Founded.Length > 100)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }
                bool hasDuplicateManufacturerName = validManufacturers.Any(x => x.ManufacturerName == manufacturerDto.ManufacturerName);

                if (hasDuplicateManufacturerName)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }
                Manufacturer manufacturer = new Manufacturer
                {
                    ManufacturerName = manufacturerDto.ManufacturerName,
                    Founded = manufacturerDto.Founded
                };
                var manufacturerFounded = manufacturerDto.Founded.Split(",", StringSplitOptions.RemoveEmptyEntries);
                string townName = "";
                string countryName = "";

                for (int i = 0; i < manufacturerFounded.Length; i++)
                {
                    if (i == manufacturerFounded.Length - 2)
                    {
                        townName = manufacturerFounded.ElementAt(i).TrimEnd();
                    }
                    if (i == manufacturerFounded.Length - 1)
                    {
                        countryName = manufacturerFounded.ElementAt(i).TrimEnd();
                    }
                }

                sb.AppendLine(string.Format(SuccessfulImportManufacturer, manufacturer.ManufacturerName, townName, countryName));
                validManufacturers.Add(manufacturer);
            }
            context.Manufacturers.AddRange(validManufacturers);
            context.SaveChanges();
            return sb.ToString().TrimEnd();
        }

        public static string ImportShells(ArtilleryContext context, string xmlString)
        {
            StringBuilder sb = new StringBuilder();
            XmlRootAttribute xmlRoot = new XmlRootAttribute("Shells");
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ImportShellDto[]), xmlRoot);

            using StringReader stringReader = new StringReader(xmlString);
            ImportShellDto[] shellDtos = (ImportShellDto[])xmlSerializer.Deserialize(stringReader);

            HashSet<Shell> validShells = new HashSet<Shell>();

            foreach (var shellDto in shellDtos)
            {
                if (double.Parse(shellDto.ShellWeight) < 2.00 || double.Parse(shellDto.ShellWeight) > 1680.00)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }
                if(shellDto.Caliber.Length < 4 || shellDto.Caliber.Length > 30
                    || string.IsNullOrEmpty(shellDto.Caliber))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Shell shell = new Shell
                {
                    ShellWeight = double.Parse(shellDto.ShellWeight),
                    Caliber = shellDto.Caliber
                };
                sb.AppendLine(string.Format(SuccessfulImportShell, shell.Caliber, shell.ShellWeight));
                validShells.Add(shell);
                context.Shells.Add(shell);
                context.SaveChanges();
            }
            return sb.ToString().TrimEnd();
        }

        public static string ImportGuns(ArtilleryContext context, string jsonString)
        {
            StringBuilder sb = new StringBuilder();

            IEnumerable<ImportGunDto> gunDtos = JsonConvert.DeserializeObject<IEnumerable<ImportGunDto>>(jsonString);
            HashSet<Gun> validGuns = new HashSet<Gun>();

            foreach (var gunDto in gunDtos)
            {
                if(gunDto.GunWeight < 100 || gunDto.GunWeight > 1350000 || gunDto.BarrelLength < 2.00
                    || gunDto.BarrelLength > 35.00 || gunDto.Range < 1 || gunDto.Range > 100000)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }
                if(!IsValid(gunDto.GunType))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Gun gun = new Gun
                {
                    ManufacturerId = gunDto.ManufacturerId,
                    GunWeight = gunDto.GunWeight,
                    BarrelLength = gunDto.BarrelLength,
                    NumberBuild = gunDto.NumberBuild,
                    Range = gunDto.Range,
                    GunType = Enum.Parse<GunType>(gunDto.GunType),
                    ShellId = gunDto.ShellId,
                };

                if (gunDto.Countries.Any() == false)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }
                else
                {
                    foreach (var gunCountry in gunDto.Countries)
                    {
                        gun.CountriesGuns.Add(new CountryGun { CountryId = gunCountry.Id });
                    }
                }

                validGuns.Add(gun);
                sb.AppendLine(string.Format(SuccessfulImportGun, gun.GunType, gun.GunWeight, gun.BarrelLength));
            }
            context.Guns.AddRange(validGuns);
            context.SaveChanges();
            return sb.ToString().TrimEnd();
        }
        private static bool IsValid(object obj)
        {
            var validator = new ValidationContext(obj);
            var validationRes = new List<ValidationResult>();

            var result = Validator.TryValidateObject(obj, validator, validationRes, true);
            return result;
        }
    }
}
