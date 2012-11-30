using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MultiZonePlayer
{
    public class RemotePipiCommand
    {
        public int CommandId;
        public String CommandName = null;
        public String CommandCode = null;
        public String[] commandCodeAlternate = null;

        public RemotePipiCommand(int commandId, String commandName, string commandCode, String[] commandCodeAlternate)
        {
            this.CommandId = commandId;
            this.CommandName = commandName;
            this.CommandCode = commandCode;
            this.commandCodeAlternate = commandCodeAlternate;
        }

        public override string  ToString()
        {
            return this.CommandName;
        }
    }

    public class RemotePipi
    {
        public static RemotePipiCommand CMD_KEY_1 = new RemotePipiCommand(1, "1", "D1", new String[] {"Oemcomma", "Tab"});
        public static RemotePipiCommand CMD_KEY_2 = new RemotePipiCommand(2, "2", "D2", new String[] {"A","Shift, A"});
        public static RemotePipiCommand CMD_KEY_3 = new RemotePipiCommand(3, "3", "D3", new String[] { "D", "Shift, D" });
        public static RemotePipiCommand CMD_KEY_4 = new RemotePipiCommand(4, "4", "D4", new String[] { "G", "Shift, G" });
        public static RemotePipiCommand CMD_KEY_5 = new RemotePipiCommand(5, "5", "D5", new String[] { "J", "Shift, J" });
        public static RemotePipiCommand CMD_KEY_6 = new RemotePipiCommand(6, "6", "D6", new String[] { "M", "Shift, M" });
        public static RemotePipiCommand CMD_KEY_7 = new RemotePipiCommand(7, "7", "D7", new String[] { "P", "Shift, P","Control, O" });
        public static RemotePipiCommand CMD_KEY_8 = new RemotePipiCommand(8, "8", "D8", new String[] { "T", "Shift, T" });
        public static RemotePipiCommand CMD_KEY_9 = new RemotePipiCommand(9, "9", "D9", new String[] {"W","Shift, W","Escape"});
        public static RemotePipiCommand CMD_KEY_0 = new RemotePipiCommand(10, "0", "D0", new String[]{" ","Space", "Alt, Tab"});
        public static RemotePipiCommand CMD_KEY_RADIO = new RemotePipiCommand(11, "RADIO", "Control, A", new String[]{"Control, Alt, D", "Multiply"});
        public static RemotePipiCommand CMD_KEY_TV = new RemotePipiCommand(12, "TV", "Control, T", null);
        public static RemotePipiCommand CMD_KEY_DVD = new RemotePipiCommand(13, "DVD", "Control, N", null);
        public static RemotePipiCommand CMD_KEY_MUSIC = new RemotePipiCommand(14, "MUSIC", "Control, M", new String[]{"Control, Alt, A", "Divide"});
        public static RemotePipiCommand CMD_KEY_PHOTO = new RemotePipiCommand(15, "PHOTO", "Control, I", new String[] { "Control, Alt, C" });
        public static RemotePipiCommand CMD_KEY_VIDEO = new RemotePipiCommand(16, "VIDEO", "Control, E", new String[] { "Control, Alt, B" });
        public static RemotePipiCommand CMD_KEY_DVD_MENU = new RemotePipiCommand(17, "DVDMENU", "Control, V", null);
        public static RemotePipiCommand CMD_KEY_UP = new RemotePipiCommand(18, "UP", "Up", null);
        public static RemotePipiCommand CMD_KEY_MUTE = new RemotePipiCommand(19, "MUTE", "F8", new String[]{"VolumeMute"});
        public static RemotePipiCommand CMD_KEY_LEFT = new RemotePipiCommand(20, "LEFT", "Left", null);
        public static RemotePipiCommand CMD_KEY_RETURN = new RemotePipiCommand(21, "ENTER", "Return", null);
        public static RemotePipiCommand CMD_KEY_RIGHT = new RemotePipiCommand(22, "RIGHT", "Right", null);
        public static RemotePipiCommand CMD_KEY_BACK = new RemotePipiCommand(23, "BACK", "Back", null);
        public static RemotePipiCommand CMD_KEY_DOWN = new RemotePipiCommand(24, "DOWN", "Down", null);
        public static RemotePipiCommand CMD_KEY_GUIDE = new RemotePipiCommand(25, "GUIDE", "F1", null);
        public static RemotePipiCommand CMD_KEY_CHUP = new RemotePipiCommand(26, "CHUP", "PageUp", null);
        public static RemotePipiCommand CMD_KEY_CHDWN = new RemotePipiCommand(27, "CHDOWN", "Next", null);
        public static RemotePipiCommand CMD_KEY_RECORD = new RemotePipiCommand(28, "RECORD", "Control, R", null);
        public static RemotePipiCommand CMD_KEY_REPEAT = new RemotePipiCommand(29, "REPEAT", "Control, Shift, R", null);//new String[] { "Back" });
        public static RemotePipiCommand CMD_KEY_PLAY = new RemotePipiCommand(30, "PLAY", "Control, Shift, P", null);
        public static RemotePipiCommand CMD_KEY_PAUSE = new RemotePipiCommand(31, "PAUSE", "Control, P", new String[]{"MediaPlayPause"});
        public static RemotePipiCommand CMD_KEY_STOP = new RemotePipiCommand(32, "STOP", "Control, S", new String[]{"MediaStop", "NumPad0", "Insert"});
        public static RemotePipiCommand CMD_KEY_REWIND = new RemotePipiCommand(33, "REWIND", "Control, Shift, D", null);
        public static RemotePipiCommand CMD_KEY_FFWD = new RemotePipiCommand(34, "FFWD", "Control, Shift, F", null);
        public static RemotePipiCommand CMD_KEY_PREVIOUS = new RemotePipiCommand(35, "PREVIOUS", "Control, B", new String[] { "MediaPreviousTrack", "Delete", "Decimal" });
        public static RemotePipiCommand CMD_KEY_NEXT = new RemotePipiCommand(36, "NEXT", "Control, F", new String[]{"MediaNextTrack"});
        public static RemotePipiCommand CMD_KEY_VOLUMEDOWN = new RemotePipiCommand(37, "VOLUMEDOWN", "VolumeDown", new String[] { "Subtract" });
        public static RemotePipiCommand CMD_KEY_VOLUMEUP = new RemotePipiCommand(38, "VOLUMEUP", "VolumeUp", new String[]{"Add"});
        //specific only to PCREMOTE type
        public static RemotePipiCommand CMD_KEY_FULLSCREEN = new RemotePipiCommand(38, "FULLSCREEN", "Alt, Return", null);

        private static RemotePipiCommand[] remoteCmdList = new RemotePipiCommand[] 
            { 
                CMD_KEY_1, CMD_KEY_2, CMD_KEY_3, CMD_KEY_4, CMD_KEY_5, CMD_KEY_6, CMD_KEY_7, CMD_KEY_8, CMD_KEY_9, CMD_KEY_0,
                CMD_KEY_RADIO, CMD_KEY_TV, CMD_KEY_DVD, CMD_KEY_MUSIC, CMD_KEY_PHOTO, CMD_KEY_VIDEO, CMD_KEY_DVD_MENU, CMD_KEY_UP,
                CMD_KEY_MUTE, CMD_KEY_LEFT, CMD_KEY_RETURN, CMD_KEY_RIGHT, CMD_KEY_BACK, CMD_KEY_DOWN, CMD_KEY_GUIDE, CMD_KEY_CHUP, 
                CMD_KEY_CHDWN, CMD_KEY_RECORD, CMD_KEY_REPEAT, CMD_KEY_PLAY, CMD_KEY_PAUSE, CMD_KEY_STOP, CMD_KEY_REWIND, 
                CMD_KEY_FFWD, CMD_KEY_PREVIOUS, CMD_KEY_NEXT, CMD_KEY_VOLUMEDOWN, CMD_KEY_VOLUMEUP,
                CMD_KEY_FULLSCREEN
            };

        public static RemotePipiCommand GetCommandByCode(String cmdCode)
        {
            for (int i = 0; i < remoteCmdList.Length; i++)
            {
                if ((remoteCmdList[i].CommandCode.ToUpper() == cmdCode.ToUpper()))
                    return remoteCmdList[i];
                else
                {
                    if (remoteCmdList[i].commandCodeAlternate != null)
                    {
                        for (int r = 0; r < remoteCmdList[i].commandCodeAlternate.Length; r++)
                        {
                            if (remoteCmdList[i].commandCodeAlternate[r] == cmdCode)
                                return remoteCmdList[i];
                        }
                    }
                }
            }
            return null;
        }
        public static RemotePipiCommand GetCommandByName(String cmdName)
        {
            for (int i = 0; i < remoteCmdList.Length; i++)
            {
                if ((remoteCmdList[i].CommandName.ToUpper() == cmdName.ToUpper()))
                    return remoteCmdList[i];
            }
            return null;
        }


        public static RemotePipiCommand GetCommand(KeyEventArgs e)
        {
            String cmdCode = GetCodeFriendlyName(e);
            return GetCommandByCode(cmdCode);
        }

        public static String GetCodeFriendlyName(KeyEventArgs e)
        {
            return e.KeyData.ToString();
        }

        public static bool IsNumeric(RemotePipiCommand cmd)
        {
            return ((cmd.CommandId >=1) && (cmd.CommandId<=10));
        }
    }
}
