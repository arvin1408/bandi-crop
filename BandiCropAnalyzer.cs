using System;
using MathWorks.MATLAB.NET.Arrays;
using BandiCropAnalyzer;

namespace ConsoleApplication1
{

    class BandiCrop_Analyzer
    {
        static void Main(string[] args)
        {
            CropAnalyzer crop_analyzer = new CropAnalyzer();

            // Initialize checkers
            String[] checker_names = { "vpd_check", "dewpoint_check", "crop_temp_check"} ;
            MWStructArray checker_list = new MWStructArray(1, 1, checker_names);

            // Initialize required input: temp_c_in, rh_in
            MWNumericArray temp_c_in = null;
            MWNumericArray rh_in = null;

            // Initialize optional inputs (by pair): verbose, TRUE/FALSE, vpd_min, vpd_min_val, vpd_max, vpd_max_val, dewtemp_offset, dewtemp_offset_val
            //MWCharArray vpd = "vpd", dewpoint = "dewpoint";
            MWCharArray checkers = "checkers", verbose = "verbose", crop_name = "crop_name", crop_name_select = "";
            MWCharArray vpd_min = "vpd_min", vpd_max = "vpd_max", dewtemp_offset = "dewtemp_offset";
            MWLogicalArray TRUE = new MWLogicalArray(true), FALSE = new MWLogicalArray(false);
            MWNumericArray vpd_min_val = 0.5, vpd_max_val = 1.2, dewtemp_offset_val = 1.0;


            // initialize output struct fields - FOR REFERENCE
            String[] vpd_check_fields = { "type", "code", "state", "vpd", "vpd_min", "vpd_max", "hum_adj", "adjust_unit" };
            String[] dewpoint_check_fields = { "type", "code", "state", "temp", "temp_dew", "dewtemp_offset", "adjust", "adjust_unit" };
            String[] crop_temp_check_fields = { "type", "code", "state", "temp", "temp_min", "temp_max", "adjust", "adjust_unit" };

            // Set checker options
            checker_list.SetField("vpd_check", TRUE);
            checker_list.SetField("dewpoint_check", TRUE);
            checker_list.SetField("crop_temp_check", TRUE);

            // Load crop data
            String path = "C:\\Program Files\\NareTrends\\BandiCropAnalyzer\\application\\bandicrop_data.ini" ;
            MWCharArray crop_inifile = path ;
            MWStructArray crops = (MWStructArray) crop_analyzer.load_crop_data(crop_inifile);

            // Create struct for output
            MWStructArray result = new MWStructArray();
            MWNumericArray result_code = null;

            try
            {
                // Full usage: crop.env_check(crops, temp_c_in, rh_in, crop_name, crop_name_select, checkers, checker_list, vpd_min, vpd_min_val, vpd_max, vpd_max_val, verbose, FALSE);
                crop_name_select = "STRAWBERRY";
                temp_c_in = 3; // deg Celsius
                rh_in = 40; // %
                 
                result = (MWStructArray)crop_analyzer.env_check(crops, temp_c_in, rh_in, crop_name, crop_name_select, checkers, checker_list);
                result_code = (MWNumericArray) result.GetField("code");

                Console.WriteLine("RESULT CODE: " + result_code);
                Console.Write("\nPress any key to exit.");
                Console.ReadKey();
            }
            catch
            {
                throw;
            }
        }
    }
}
