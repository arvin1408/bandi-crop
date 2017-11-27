using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MathWorks.MATLAB.NET.Arrays;
using BandiCropAnalyzer;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            CropAnalyzer crop = new CropAnalyzer();
            
            // Initialize required input: temp_c_in, rh_in
            MWNumericArray temp_c_in = null;
            MWNumericArray rh_in = null;

            // Initialize optional input (by pair): verbose, TRUE/FALSE, vpd_min, vpd_min_val, vpd_max, vpd_max_val
            MWCharArray verbose = "verbose", vpd_min = "vpd_min", vpd_max = "vpd_max";
            MWLogicalArray TRUE = new MWLogicalArray(true), FALSE = new MWLogicalArray(false);
            MWNumericArray vpd_min_val = 0.5, vpd_max_val = 1.2;

            // initialize output struct fields
            String[] result_fields = { "type", "code", "state", "vpd", "vpd_min", "vpd_max", "hum_adj", "adjust_unit"};
            MWStructArray result = new MWStructArray(1, result_fields.Count(), result_fields);
            //MWStructArray result = new MWStructArray();
            //MWNumericArray result_code = null;


            try
            {
                // Full usage: moisture.vpd_check(temp_c_in, rh_in, verbose, FALSE, vpd_min, vpd_min_val, vpd_max, vpd_max_val);
                temp_c_in = 25; // deg Celsius
                rh_in = 70; // %
                vpd_min_val = 0.5; // kPa
                vpd_max_val = 1.2; // kPa
                 
                result = (MWStructArray) crop.vpd_check(temp_c_in, rh_in, verbose, TRUE, vpd_min, vpd_min_val, vpd_max, vpd_max_val);
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
