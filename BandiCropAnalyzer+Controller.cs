using System;
using System.Windows.Forms;
using MathWorks.MATLAB.NET.Arrays;
using BandiCropAnalyzer;
using CES;


class Program
{ 
    static void Main()
    {
        BandiCropModule BCM = new BandiCropModule();

        //ConnectEyesen ce = new ConnectEyesen();
        //DevInfo dev = new DevInfo();

        // TEST CONNECTION
        //ce.ConnectToServer("192.168.1.222", 7777);
        //Console.WriteLine(ce.RecieveMassage());

        // RUN TEST
        //BCM.run_module_test();

        // LOAD CROP DATA
        MWStructArray crops_db = BCM.load_crop_db();

        // TEST DONG OBJECT TO APPLY CONTROLS
        DongConfig nare_testbed = new DongConfig(1);

        nare_testbed.name = "NARE_TESTBED";
        nare_testbed.fan_total = 5;
        nare_testbed.ch_exhaust_fans = new int[4] { 1, 2, 3, 4 };
        nare_testbed.ch_ceiling_vents_L1 = new int[1] { 1 };
        nare_testbed.ch_side_vents_L1 = new int[1] { 2 };
        nare_testbed.ch_heater = new int[1] { 3 };


        // SPECIFY DONG STATE AND CROP
        string crop_name = "TOMATO";
        DongState dong_state = new DongState();
        dong_state.temp_in = 4;
        dong_state.temp_out = -3;
        dong_state.hum_in = 50;
        dong_state.hum_out = 40;
        dong_state.hum_bed = 30;
        dong_state.co2_supply = true;
        dong_state.rain = true;

        // CHECK CROP STATUS
        MWStructArray env_status = BCM.crop_env_checker(crops_db, crop_name, dong_state);

        // CHECK FOR REQUIRED CONTROL ACTIONS
        MWStructArray control_actions = BCM.crop_env_controller(crops_db, crop_name, dong_state);

        // APPLY CONTROL TO NARE_TESTBED
        BCM.autoswitch_control(control_actions, nare_testbed);

        Console.WriteLine("\nPress any key to exit.");
        Console.ReadKey();
    }

}


public class BandiCropModule
{
    public MWStructArray load_crop_db()
    {
        CropAnalyzer crop_analyzer = new CropAnalyzer();

        // LOAD CROP DATA
        String path = "C:\\Program Files\\NareTrends\\BandiCropAnalyzer\\application\\bandicrop_data.ini";
        MWCharArray crop_inifile = path;
        MWStructArray crops_db = (MWStructArray)crop_analyzer.load_crop_data(crop_inifile);
        return crops_db;
    }

    public void run_module_test()
    {
        BandiCropAnalyzer.CropAnalyzer crop_analyzer = new BandiCropAnalyzer.CropAnalyzer(); 

        // TEST CROP AND SENSOR FEED
        string crop_name = "STRAWBERRY";
        DongState dong_state = new DongState();
        dong_state.temp_in = 4;
        dong_state.temp_out = -3;
        dong_state.hum_in = 50;
        dong_state.hum_out = 40;
        dong_state.hum_bed = 30;
        dong_state.co2_supply = true;
        dong_state.rain = true;

        // TEST DONG OBJECT TO APPLY CONTROLS
        DongConfig dong = new DongConfig(1)
        {
            name = "NARE_DONG",
            fan_total = 5,
        };

        MWStructArray crops_db = load_crop_db();

        // DO CROP ENVIRONMENT CHECK ONLY
        MWStructArray env_status = crop_env_checker(crops_db, crop_name, dong_state);

        // PARSE CHECK RESULTS FOR DISPLAY
        int print_level = 2; // 0->1->2 : INCREASING VERBOSITY
        string result_out = parse_results(env_status, print_level);

        // PERFORM STATUS CHECK + CONTROL ACTION
        MWStructArray control_actions = crop_env_controller(crops_db, crop_name, dong_state);

        // APPLY CONTROL TO DONG1
        autoswitch_control(control_actions, dong);

        Console.WriteLine("\nMODULE TEST COMPLETE. PRESS ANY KEY TO CONTINUE...");
        Console.ReadKey();
    }

    // DO CONTROL ACTION BASED ON ENVIRONMENT CHECKING OUTPUT
    public MWStructArray crop_env_controller(MWStructArray crops_db, string crop, DongState sensors)

    {
        Console.WriteLine("\n======================= CROP CONTROLLER OUTPUT =======================");
        BandiCropAnalyzer.CropAnalyzer crop_controller = new BandiCropAnalyzer.CropAnalyzer();

        // Initialize checkers
        String[] checker_names = { "vpd_check", "dewpoint_check", "crop_temp_check" };
        MWStructArray checker_list = new MWStructArray(1, 1, checker_names);

        // Initialize optional inputs (by pair): verbose, TRUE/FALSE, vpd_min, vpd_min_val, vpd_max, vpd_max_val, dewtemp_offset, dewtemp_offset_val
        //MWCharArray vpd = "vpd", dewpoint = "dewpoint";
        MWCharArray checkers = "checkers", verbose = "verbose", crop_name = "crop_name", crop_name_select = "";
        MWCharArray rh_bed = "rh_bed", co2_supply = "co2_supply", rain = "rain";
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
            MWNumericArray temp_c_in = sensors.temp_in; // deg Celsius
            MWNumericArray rh_in = sensors.hum_in; // %
            MWNumericArray temp_c_out =  sensors.temp_out; // deg Celsius
            MWNumericArray rh_out = sensors.hum_out; // %
            MWNumericArray hum_bed = sensors.hum_bed; // %
            MWLogicalArray co2_state = sensors.co2_supply; // %
            MWLogicalArray rain_state = sensors.rain; // %

            return control_action = (MWStructArray)crop_controller.env_controller(crops_db, crop_name_select,
                                                                            temp_c_in, rh_in, temp_c_out, rh_out,
                                                                            rh_bed, hum_bed,
                                                                            co2_supply, co2_state,
                                                                            rain, rain_state);

        }
        catch
        {
            throw;
        }

    }

    // IMPLEMENTING THE ENVIRONMENT CHECK ROUTINES
    public MWStructArray crop_env_checker(MWStructArray crops_db, string crop, DongState sensors)
    {
        //Console.WriteLine("\n======================= ENVIRONMENT ANALYSIS =======================");

        BandiCropAnalyzer.CropAnalyzer crop_analyzer = new BandiCropAnalyzer.CropAnalyzer();

        // Initialize checkers
        String[] checker_names = { "vpd_check", "dewpoint_check", "crop_temp_check"} ;
        MWStructArray checker_list = new MWStructArray(1, 1, checker_names);

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
            MWNumericArray temp_c_in = sensors.temp_in; // deg Celsius
            MWNumericArray rh_in = sensors.hum_in; // %
                 
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

    // PARSE ENV_CHECK OUTPUT
    public string parse_results(MWStructArray result, int print_level)
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

    // INTERFACETO HARDWARE-LEVEL COMMANDS
    public void execute_control(MWStructArray control_action, DongConfig dong)
    {
        Console.WriteLine("\n======================= SENDING CONTROL SIGNALS =======================");
        Form1 eyesen = new Form1();

        // PARSE DONG INFORMATION
        Console.WriteLine("\nACTIVATING CONTROLS FOR DONG #{0} ..." , dong.GetType().GetProperty("id").GetValue(dong, null));
        Console.WriteLine("NAME : " + dong.GetType().GetProperty("name").GetValue(dong, null));

        int nFields = control_action.NumberOfFields;
        string[] field_names = control_action.FieldNames;
        for (int ix = 0; ix < nFields; ix++)
        {
            string control_target = field_names[ix];
            string command = control_action.GetField(control_target).ToString();
            // string command = "on"; //  switch -> stop, open, close / (2) fan -> on, off / (3) on, off)
            Console.WriteLine(control_target + " : " + command);

            // GET IO POINTS FROM DONG OBJECT
            string iotype = "fan"; // EYESEN ACTUATORS: switch, fan, relay 입력 가능
            int dongNumber = dong.id;

            // HEATER
            if (control_target == "heater")
            {
                int[] channels = dong.ch_heaters;
                for (int i=0; i<channels.Length ; i++)
                {
                    eyesen.EquipControl(iotype, dongNumber, channels[i], command);
                }

            };

            // VENTS
            if (control_target == "vent")
            {
                for (int i = 0; i < channels.Length; i++)
                {
                    eyesen.EquipControl(iotype, dongNumber, channels[i], command);
                }

            };

            // VENTS
            if (control_target == "fan")
            {
                for (int i = 0; i < channels.Length; i++)
                {
                    eyesen.EquipControl(iotype, dongNumber, channels[i], command);
                }

            };

            // VENTS
            if (control_target == "irrigation")
            {
                for (int i = 0; i < channels.Length; i++)
                {
                    eyesen.EquipControl(iotype, dongNumber, channels[i], command);
                }
            };



        }
    }

    // SWITCH BETWEEN AUTOMATIC AND GROWTH CONTROLLER
    public void autoswitch_control(MWStructArray control_actions, DongConfig dong)
    {
        // CHECK IF CONTROL ACTIONS RETURNED. STOP AUTOMATIC CONTROL, ACTIVATE TOUCH PANEL CONTROL
        if (control_actions.NumberOfFields == 0)
        {
            Console.WriteLine(">> NO CONTROL ACTIONS RECEIVED.");

            // IF CONTROL ACTIONS EMPTY, REACTIVATE AUTOMATIC CONTROL
            //------------> activate automatic control here
            Console.WriteLine(">> REVERTING TO AUTOMATIC CONTROL...");

        }
        else
        {
            // IF CONTROL ACTIONS RECEIVED, SWITCH TO GROWTH CONTROL
            // ------------> deactivate automatic control here
            Console.WriteLine(">> SWITCHING TO GROWTH CONTROL...");

            // EXECUTE CONTROL ACTIONS THROUGH EYESEN INTERFACE
            execute_control(control_actions, dong);
        }
    }
}

// DONG CLASS - APPLY CONTROL ACTIONS 
public class DongConfig
{
    public int id { get; set; }
    public string name { get; set; }
    public int fan_total { get; set; }
    public bool co2_supply { get; set; }

    // RELAY, FAN, SWITCH CHANNEL SPECIFICATIONS
    public int[] ch_exhaust_fans { get; set; }

    public int[] ch_ceiling_vents_L1 { get; set; }
    public int[] ch_ceiling_vents_L2 { get; set; }
    public int[] ch_ceiling_vents_L3 { get; set; }

    public int[] ch_side_vents_L1 { get; set; }
    public int[] ch_side_vents_L2 { get; set; }
    public int[] ch_side_vents_L3 { get; set; }
    public int[] ch_side_vents_inner { get; set; }

    public int[] ch_curtain_L1 { get; set; }
    public int[] ch_curtain_L2 { get; set; }
    public int[] ch_curtain_L3 { get; set; }

    public int[] ch_heater { get; set; }
    public int[] ch_irrigation { get; set; }
    public int[] ch_humidifier { get; set; }
    public int[] ch_co2 { get; set; }


    public DongConfig(int dongNumber)
    {
        id = dongNumber;
    }
}

// SENSOR FEED CLASS - USE ENVIRONMENT ANALYSIS
public class DongState : BandiCropModule
{

    // INDOOR
    public double temp_in { get; set; }
    public double hum_in { get; set; }
    public double lux_in { get; set; }
    public double ppfd_in { get; set; }
    public double temp_bed { get; set; }
    public double hum_bed { get; set; }
    public double ec_bed { get; set; }
    public double co2 { get; set; }

    // OUTDOOR
    public double temp_out { get; set; }
    public double hum_out { get; set; }
    public double solar { get; set; }
    public double wind_speed { get; set; }
    public double wind_direction { get; set; }
    public bool rain { get; set; }

    // ACTUATOR STATES
    public bool co2_supply { get; set; }

    // CONSTRUCTORS
    public DongState() { }
}