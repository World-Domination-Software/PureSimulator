using UnityEngine;

namespace CrimsofallTechnologies.ServerSimulator
{
    public class ChassisCommandsExtension : MonoBehaviour
    {
        public Chassis chassis;

        public string PureHWList()
        {
            string m = "Name          Status     Identify     Slot    Index  Model   Part Number    Serial   Speed         Temperature      Voltage    Type        Handle                      Parent\n";
            m += "CT0           ok         off          -       0      -       -              -        10.00Gb/s     -                -          controller  platform_SN0                hwroot\n" +
                 "CT0.ETH0      ok         -            -       0      -       -              -        10.00Gb/s     -                -          eth_port    platform_SN0_0000:03.0      platform_SN0\n" +
                 "CT0.ETH1      ok         -            -       1      -       -              -        10.00Gb/s     -                -          eth_port    platform_SN0_0000:03.1      platform_SN0\n" +
                 "CT0.ETH2      ok         -            -       2      -       -              -        10.00Gb/s     -                -          eth_port    platform_SN0_0000:03.2      platform_SN0\n" +
                 "CT0.ETH3      ok         -            -       3      -       -              -        10.00Gb/s     -                -          eth_port    platform_SN0_0000:03.3      platform_SN0\n" +

                 "CT0.FAN0      ok         -            -       0      -       -              -        -             -                -          cooling     platform_SN0_CtrlFan_0      platform_SN0\n" +
                 "CT0.FAN1      ok         -            -       1      -       -              -        -             -                -          cooling     platform_SN0_CtrlFan_1      platform_SN0\n" +
                 "CT0.FAN2      ok         -            -       2      -       -              -        -             -                -          cooling     platform_SN0_CtrlFan_2      platform_SN0\n" +
                 "CT0.FAN3      ok         -            -       3      -       -              -        -             -                -          cooling     platform_SN0_CtrlFan_3      platform_SN0\n" +
                 "CT0.FAN4      ok         -            -       4      -       -              -        -             -                -          cooling     platform_SN0_CtrlFan_4      platform_SN0\n" +

                 "CT0.FC0       ok         -            6       0      -       -              -        8.00Gb/s      -                -          fc_port     platform_SN0_FakeFCPort_0   platform_SN0\n" +
                 "CT0.FC1       ok         -            6       1      -       -              -        8.00Gb/s      -                -          fc_port     platform_SN0_FakeFCPort_1   platform_SN0\n" +
                 "CT0.FC2       ok         -            6       2      -       -              -        8.00Gb/s      -                -          fc_port     platform_SN0_FakeFCPort_2   platform_SN0\n" +
                 "CT0.FC3       ok         -            6       3      -       -              -        8.00Gb/s      -                -          fc_port     platform_SN0_FakeFCPort_3   platform_SN0\n" +
                 "CT0.FC4       ok         -            6       4      -       -              -        8.00Gb/s      -                -          fc_port     platform_SN0_FakeFCPort_4   platform_SN0\n" +
                 "CT0.FC5       ok         -            7       5      -       -              -        8.00Gb/s      -                -          fc_port     platform_SN0_FakeFCPort_5   platform_SN0\n" +
                 "CT0.FC6       ok         -            7       6      -       -              -        8.00Gb/s      -                -          fc_port     platform_SN0_FakeFCPort_6   platform_SN0\n" +
                 "CT0.FC7       ok         -            7       7      -       -              -        8.00Gb/s      -                -          fc_port     platform_SN0_FakeFCPort_7   platform_SN0\n" +
                 "CT0.FC8       ok         -            7       8      -       -              -        8.00Gb/s      -                -          fc_port     platform_SN0_FakeFCPort_8   platform_SN0\n" +
                 "CT0.FC9       ok         -            8       9      -       -              -        8.00Gb/s      -                -          fc_port     platform_SN0_FakeFCPort_9   platform_SN0\n" +
                 "CT0.FC10      ok         -            8       10     -       -              -        8.00Gb/s      -                -          fc_port     platform_SN0_FakeFCPort_10  platform_SN0\n" +
                 "CT0.FC11      ok         -            8       11     -       -              -        8.00Gb/s      -                -          fc_port     platform_SN0_FakeFCPort_11  platform_SN0\n" +
                 "CT0.FC12      ok         -            8       12     -       -              -        8.00Gb/s      -                -          fc_port     platform_SN0_FakeFCPort_12  platform_SN0\n" +
                 "CT0.FC13      ok         -            9       13     -       -              -        8.00Gb/s      -                -          fc_port     platform_SN0_FakeFCPort_13  platform_SN0\n" +
                 "CT0.FC14      ok         -            9       14     -       -              -        8.00Gb/s      -                -          fc_port     platform_SN0_FakeFCPort_14  platform_SN0\n" +
                 "CT0.FC15      ok         -            9       15     -       -              -        8.00Gb/s      -                -          fc_port     platform_SN0_FakeFCPort_15  platform_SN0\n" +
                 "CT0.FC16      ok         -            9       16     -       -              -        8.00Gb/s      -                -          fc_port     platform_SN0_FakeFCPort_16  platform_SN0\n" +
                 "CT0.FC17      ok         -            10      17     -       -              -        8.00Gb/s      -                -          fc_port     platform_SN0_FakeFCPort_17  platform_SN0\n" +
                 "CT0.FC18      ok         -            10      18     -       -              -        8.00Gb/s      -                -          fc_port     platform_SN0_FakeFCPort_18  platform_SN0\n" +
                 "CT0.FC19      ok         -            10      19     -       -              -        8.00Gb/s      -                -          fc_port     platform_SN0_FakeFCPort_19  platform_SN0\n" +
                 "CT0.FC20      ok         -            10      20     -       -              -        8.00Gb/s      -                -          fc_port     platform_SN0_FakeFCPort_20  platform_SN0\n" +
                 "CT0.FC21      ok         -            10      21     -       -              -        8.00Gb/s      -                -          fc_port     platform_SN0_FakeFCPort_21  platform_SN0\n" +
                 "CT0.FC22      ok         -            11      22     -       -              -        8.00Gb/s      -                -          fc_port     platform_SN0_FakeFCPort_22  platform_SN0\n" +
                 "CT0.FC23      ok         -            11      23     -       -              -        8.00Gb/s      -                -          fc_port     platform_SN0_FakeFCPort_23  platform_SN0\n" +
                 "CT0.FC24      ok         -            11      24     -       -              -        8.00Gb/s      -                -          fc_port     platform_SN0_FakeFCPort_24  platform_SN0\n" +
                 "CT0.FC25      ok         -            11      25     -       -              -        8.00Gb/s      -                -          fc_port     platform_SN0_FakeFCPort_25  platform_SN0\n" +
                 "CT0.FC26      ok         -            11      26     -       -              -        8.00Gb/s      -                -          fc_port     platform_SN0_FakeFCPort_26  platform_SN0\n" +
                 "CT0.FC27      ok         -            12      27     -       -              -        8.00Gb/s      -                -          fc_port     platform_SN0_FakeFCPort_27  platform_SN0\n" +
                 "CT0.FC28      ok         -            12      28     -       -              -        8.00Gb/s      -                -          fc_port     platform_SN0_FakeFCPort_28  platform_SN0\n" +
                 "CT0.FC29      ok         -            12      29     -       -              -        8.00Gb/s      -                -          fc_port     platform_SN0_FakeFCPort_29  platform_SN0\n" +
                 "CT0.FC30      ok         -            12      30     -       -              -        8.00Gb/s      -                -          fc_port     platform_SN0_FakeFCPort_30  platform_SN0\n" +
                 "CT0.FC31      ok         -            12      31     -       -              -        8.00Gb/s      -                -          fc_port     platform_SN0_FakeFCPort_31  platform_SN0\n" +
                 "CT0.FC32      ok         -            13      32     -       -              -        8.00Gb/s      -                -          fc_port     platform_SN0_FakeFCPort_32  platform_SN0\n" +
                 "CT0.FC33      ok         -            13      33     -       -              -        8.00Gb/s      -                -          fc_port     platform_SN0_FakeFCPort_33  platform_SN0\n" +
                 "CT0.FC34      ok         -            13      34     -       -              -        8.00Gb/s      -                -          fc_port     platform_SN0_FakeFCPort_34  platform_SN0\n" +
                 "CT0.FC35      ok         -            13      35     -       -              -        8.00Gb/s      -                -          fc_port     platform_SN0_FakeFCPort_35  platform_SN0\n" +
                 "CT0.FC36      ok         -            14      36     -       -              -        8.00Gb/s      -                -          fc_port     platform_SN0_FakeFCPort_36  platform_SN0\n" +
                 "CT0.FC37      ok         -            14      37     -       -              -        8.00Gb/s      -                -          fc_port     platform_SN0_FakeFCPort_37  platform_SN0\n" +
                 "CT0.FC38      ok         -            14      38     -       -              -        8.00Gb/s      -                -          fc_port     platform_SN0_FakeFCPort_38  platform_SN0\n" +
                 "CT0.FC39      ok         -            14      39     -       -              -        8.00Gb/s      -                -          fc_port     platform_SN0_FakeFCPort_39  platform_SN0\n";

            if (chassis.insertedLaptopPort != null) //means the user has plugged into this server? 
            {
                for (int n = 0; n < chassis.HardDrives.Length; n++)
                {
                    if (chassis.HardDrives[n].status != HardDriveStatus.not_inserted)
                        m += $"SH0.BAY{n}      ok      -            -      {n}     -       -              -        24.00Gb/s     -                -          drive_bay   platform_SN0_Shelf0_Bay{n}  FakeShelf0\n";
                }
            }

            m += "CT0.IB0       ok         -            4       0      -       -              -        56.00Gb/s     -                -          ib_port     platform_SN0_FakeIBPort_0   platform_SN0\n" +
                 "CT0.IB1       ok         -            4       1      -       -              -        56.00Gb/s     -                -          ib_port     platform_SN0_FakeIBPort_1   platform_SN0\n";

            return m;
        }

        public string HardwareCheck()
        {
            return "==== CPU ====\nWarning! unknown 'controller' type. Skipping CPU check.\n\n==== RAM ====\nMEMTotal: 20339144 kB\n\n==== FC Targets ====\nNo FC interfaces found." +
                "\n\niSCSI Targets\nDetected 4 iSCSI capable ports\n\n==== INFINIBAND TARGETS ====\n-\n\n==== STORAGE ====\nUnknown chassis. Defaulting to _build_platinum_chassis()" +
                "\nCould not read FRU info.\n[Errno 2] No such file or directory: '/dev/i2c-15'\n\n==== RESULTS ====\nAll's Well!";
        }

        public string GetControllersList()
        {
            string d = "Name       Type                   Mode          Model       Version        Status        Internal Details\n" +
                      $"CT0        array_controller       {chassis.GetControllerState(0)}       {chassis.GetModel(0)}       {chassis.GetCurrentPurityVersion(0)}          {chassis.GetControllerStatus(0)}         -\n" +
                      $"CT1        array_controller       {chassis.GetControllerState(1)}     {chassis.GetModel(1)}       {chassis.GetCurrentPurityVersion(1)}          {chassis.GetControllerStatus(1)}         -";

            if (chassis.ConnectedShelfChassis != null)
            {
                d += $"SH{chassis.ConnectedShelfChassis.chassisIndex}.SC0    shelf_controller       -               {chassis.GetModel(0)}       {chassis.GetCurrentPurityVersion(0)}          -         -\n" +
                     $"SH{chassis.ConnectedShelfChassis.chassisIndex}.SC1    shelf_controller       -               {chassis.GetModel(1)}       {chassis.GetCurrentPurityVersion(1)}          -         -";
            }

            return d;
        }

        public string PureNetworkList()
        {
            string d = "Name        Enabled   Speed        Services\n" +
                       //"CT0.FC0     True      8.00Gb/s     scsi-fc\n" +
                       //"CT0.FC1     True      8.00Gb/s     scsi-fc\n" +
                       //"CT0.FC2     True      8.00Gb/s     scsi-fc\n" +
                       //"CT0.FC3     True      8.00Gb/s     scsi-fc\n" +
                       //"CT0.FC4     True      8.00Gb/s     scsi-fc\n" +

                       "vir0        True      10.00Gb/s    management\n" +
                       "vir1        False     10.00Gb/s    management\n" +
                       "ct0.eth0    True      10.00Gb/s    management\n" +
                       "ct0.eth1    True      10.00Gb/s    management\n" +
                       "ct0.eth2    True      10.00Gb/s    replication\n" +
                       "ct0.eth3    True      10.00Gb/s    replication\n";

            return d;
        }

        public string FindAndListFiles(string dir)
        {
            string s = "."; //means directory is empty!
            string[] spl = dir.Split('/');

            //files on controller/mounted directory:
            if(dir == "")
            {
                return chassis.GetCurrentController().Dir.GetFilesNames();
            }

            //files in /mnt directory
            if(dir == "/mnt" && chassis.commandProcessor.Mounted)
            {
                return chassis.InsertedUsbPort.Dir.GetFilesNames();
            }

            //searching USB drive directory - does not work in linux
            /*char[] c = new char[] { '/','d','e','v','/' };
            if(dir.TrimStart(c) == chassis.InsertedUsbPort.Dir.DirectoryName) {
                return chassis.InsertedUsbPort.Dir.GetFilesNames();
            }*/

            //find and list all directories with pattern:
            if(spl[1] == "dev")
            {
                if(spl.Length >= 3) //search via pattern
                {
                    if(spl[2] == "") //like ls /dev/
                        return chassis.InsertedUsbPort.Dir.DirectoryName;

                    char[] pattern = spl[2].ToCharArray();
                    string d = "*";
                    if(chassis.InsertedUsbPort!=null && pattern.Length >= 4 && pattern[0] == 's' && 
                        pattern[1] == 'd' && pattern[2] == '*' && pattern[3] == '1') {
                        d = chassis.InsertedUsbPort.Dir.DirectoryName;
                    }
                    return d;
                }
                else if(chassis.InsertedUsbPort != null)
                {
                    //return connected drive
                    return chassis.InsertedUsbPort.Dir.DirectoryName;
                }
            }

            return s;
        }
    }
}