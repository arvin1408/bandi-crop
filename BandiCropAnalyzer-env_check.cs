using System;
using MathWorks.MATLAB.NET.Arrays;
using BandiCropAnalyzer;

namespace Bandi_Complex
{

    class BandiCrop_Analyzer
    {
        static void Main(string[] args)
        {
            CropAnalyzer crop_analyzer = new CropAnalyzer();
            MWStructArray result = new MWStructArray();

            // LOAD CROP DATA
            String path = "C:\\Program Files\\NareTrends\\BandiCropAnalyzer\\application\\bandicrop_data.ini";
            MWCharArray crop_inifile = path;
            MWStructArray crops_db = (MWStructArray) crop_analyzer.load_crop_data(crop_inifile);

            string crop_name = "STRAWBERRY";

            // DO CROP ENVIRONMENT CHECK
            double temp = 4;
            double hum = 80;
            result = (MWStructArray) crop_env_checker(crops_db, temp, hum, crop_name);

            // PARSE RESULTS FOR DISPLAY
            int print_level = 2; // 0->1->2 : INCREASING VERBOSITY
            string result_out = parse_results(result, print_level);

            // PERFORM STATUS CHECK + CONTROL ACTION
            double temp_in = 4;
            double hum_in = 80;
            double temp_out = -3;
            double hum_out = 40;
            result = (MWStructArray)crop_env_controller(crops_db, crop_name, temp_in, hum_in, temp_out, hum_out);

            Console.WriteLine("\nPress any key to exit.");
            Console.ReadKey();

        }

        // METHOD TO DO CONTROL ACTION BASED ON STATUS OUTPUT
        public static MWStructArray crop_env_controller(MWStructArray crops_db, string crop, double temp_in, double hum_in, double temp_out, double hum_out)
        {
            Console.WriteLine("\n======================= CROP CONTROLLER OUTPUT =======================");
            CropAnalyzer crop_controller = new CropAnalyzer();

            // Initialize checkers
            String[] checker_names = { "vpd_check", "dewpoint_check", "crop_temp_check" };
            MWStructArray checker_list = new MWStructArray(1, 1, checker_names);

            // Initialize required input: temp_c_in, rh_in, temp_c_out, rh_out
            MWNumericArray temp_c_in = null;
            MWNumericArray rh_in = null;
            MWNumericArray temp_c_out = null;
            MWNumericArray rh_out = null;

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

            // Create struct for output
            MWStructArray control_action = new MWStructArray();

            try
            {
                // Full usage: crop.env_check(crops, temp_c_in, rh_in, crop_name, crop_name_select, checkers, checker_list, vpd_min, vpd_min_val, vpd_max, vpd_max_val, verbose, FALSE);
                crop_name_select = crop;
                temp_c_in = temp_in; // deg Celsius
                rh_in = hum_in; // %
                temp_c_out = temp_out; // deg Celsius
                rh_out = hum_out; // %

                control_action= (MWStructArray)crop_controller.env_controller(crops_db, crop_name_select, temp_c_in, rh_in, temp_c_out, rh_out);

                Console.WriteLine("CONTROL ACTION: \n" + control_action);

                //Console.WriteLine("Press any key to exit.");
                //Console.ReadKey();

                return (control_action);
            }
            catch
            {
                throw;
            }

        }

        // METHOD IMPLEMENTING THE ENVIRONMENT CHECK ROUTINES
        public static MWStructArray crop_env_checker(MWStructArray crops_db, double temp, double hum, string crop)
        {
            Console.WriteLine("\n======================= ENVIRONMENT ANALYSIS =======================");

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

            // Create struct for output
            MWStructArray result = new MWStructArray();
            MWNumericArray result_code = null;

            try
            {
                // Full usage: crop.env_check(crops, temp_c_in, rh_in, crop_name, crop_name_select, checkers, checker_list, vpd_min, vpd_min_val, vpd_max, vpd_max_val, verbose, FALSE);
                crop_name_select = crop;
                temp_c_in = temp; // deg Celsius
                rh_in = hum; // %
                 
                result = (MWStructArray) crop_analyzer.env_check(crops_db, temp_c_in, rh_in, crop_name, crop_name_select, checkers, checker_list);
                result_code = (MWNumericArray) result.GetField("code");

                Console.WriteLine("RESULT CODE: " + result_code);
                //Console.WriteLine("Press any key to exit.");
                //Console.ReadKey();

                return (result);
            }
            catch
            {
                throw;
            }
        }

        // METHOD TO PARSE ENV_CHECK OUTPUT
        public static string parse_results(MWStructArray result, int print_level)
        {
            Console.WriteLine("\n======================= ENVIRONMENT CHECK RESULT =======================");
            String[] checker_names = { "vpd_check", "dewpoint_check", "crop_temp_check" };

            // READ OUTPUT DETAILS
            MWStructArray result_checker = new MWStructArray();
            MWNumericArray result_code = null;
            MWNumericArray checker_code = null;
            MWCharArray result_type = null;
            MWCharArray result_state = null;
            MWCharArray result_checkvar_name = null;
            MWNumericArray result_checkvar_value = null;
            MWCharArray result_checkvar_unit = null;
            MWNumericArray result_adjust = null;
            MWCharArray result_adjust_unit = null;

            result_code = (MWNumericArray)result.GetField("code");
            int nFields = result.NumberOfFields - 1;
            string result_out = null;

            for (int ifield = 1; ifield <= nFields; ifield++)
            {
                result_checker = (MWStructArray)result.GetField(checker_names[ifield - 1]);
                checker_code = (MWNumericArray)result_checker.GetField("code");

                switch (print_level)
                {
                    case 0: // PRINT ABNORMAL RESULTS
                        if ((int)checker_code > 0)
                        {
                            result_type = (MWCharArray)result_checker.GetField("type");
                            result_state = (MWCharArray)result_checker.GetField("state");
                            result_out = "\n" + result_type.ToString() + " STATUS : " + result_state.ToString();
                            Console.WriteLine(result_out);
                        }
                        break;
                    case 1: //  PRINT ABNORMAL RESULTS + DETAILS
                        if ((int)checker_code > 0)
                        {
                            result_type = (MWCharArray)result_checker.GetField("type");
                            result_state = (MWCharArray)result_checker.GetField("state");
                            result_checkvar_name = (MWCharArray)result_checker.GetField("checkvar_name");
                            result_checkvar_value = (MWNumericArray)result_checker.GetField("checkvar_value");
                            result_checkvar_unit = (MWCharArray)result_checker.GetField("checkvar_unit");
                            result_adjust = (MWNumericArray)result_checker.GetField("adjust");
                            result_adjust_unit = (MWCharArray)result_checker.GetField("adjust_unit");

                            result_out = "\n" + result_type.ToString() + " STATUS : " + result_state.ToString();
                            result_out += "\n" + result_checkvar_name.ToString() + " : " + result_checkvar_value.ToString() + " " + result_checkvar_unit.ToString();
                            result_out += "\n" + "ADJUST : " + result_adjust.ToString() + " " + result_adjust_unit.ToString();

                            Console.WriteLine(result_out);
                        }
                        break;

                    case 2: //  PRINT ALL RESULTS + DETAILS

                        result_type = (MWCharArray)result_checker.GetField("type");
                        result_state = (MWCharArray)result_checker.GetField("state");
                        result_checkvar_name = (MWCharArray)result_checker.GetField("checkvar_name");
                        result_checkvar_value = (MWNumericArray)result_checker.GetField("checkvar_value");
                        result_checkvar_unit = (MWCharArray)result_checker.GetField("checkvar_unit");
                        result_adjust = (MWNumericArray)result_checker.GetField("adjust");
                        result_adjust_unit = (MWCharArray)result_checker.GetField("adjust_unit");

                        result_out = "\n" + result_type.ToString() + " STATUS : " + result_state.ToString();
                        result_out += "\n" + result_checkvar_name.ToString() + " : " + result_checkvar_value.ToString() + " " + result_checkvar_unit.ToString();
                        result_out += "\n" + "ADJUST : " + result_adjust.ToString() + " " + result_adjust_unit.ToString();

                        Console.WriteLine(result_out);
                        break;
                } // end of switch statement

            }

            return (result_out);
        }

    }
}