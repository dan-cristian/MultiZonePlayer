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
        public static RemotePipiCommand CMD_KEY_1 = new RemotePipiCommand(1, "1", "d1", null);
        public static RemotePipiCommand CMD_KEY_2 = new RemotePipiCommand(2, "2", "d2", null);
        public static RemotePipiCommand CMD_KEY_3 = new RemotePipiCommand(3, "3", "d3", null);
        public static RemotePipiCommand CMD_KEY_4 = new RemotePipiCommand(4, "4", "d4", null);
        public static RemotePipiCommand CMD_KEY_5 = new RemotePipiCommand(5, "5", "d5", null);
        public static RemotePipiCommand CMD_KEY_6 = new RemotePipiCommand(6, "6", "d6", null);
        public static RemotePipiCommand CMD_KEY_7 = new RemotePipiCommand(7, "7", "d7", null);
        public static RemotePipiCommand CMD_KEY_8 = new RemotePipiCommand(8, "8", "d8", null);
        public static RemotePipiCommand CMD_KEY_9 = new RemotePipiCommand(9, "9", "d9", null);
        public static RemotePipiCommand CMD_KEY_0 = new RemotePipiCommand(10, "0", "d0", null);
        public static RemotePipiCommand CMD_KEY_RADIO = new RemotePipiCommand(11, "radio", "control, a", new String[]{"control, alt, d", "multiply"});
        public static RemotePipiCommand CMD_KEY_TV = new RemotePipiCommand(12, "tv", "control, t", null);
        public static RemotePipiCommand CMD_KEY_DVD = new RemotePipiCommand(13, "dvd", "control, n", null);
        public static RemotePipiCommand CMD_KEY_MUSIC = new RemotePipiCommand(14, "music", "control, m", new String[]{"control, alt, a", "divide"});
        public static RemotePipiCommand CMD_KEY_PHOTO = new RemotePipiCommand(15, "photo", "control, i", new String[] { "control, alt, c" });
        public static RemotePipiCommand CMD_KEY_VIDEO = new RemotePipiCommand(16, "video", "control, e", new String[] { "control, alt, b" });
        public static RemotePipiCommand CMD_KEY_DVD_MENU = new RemotePipiCommand(17, "dvdmenu", "control, v", null);
        public static RemotePipiCommand CMD_KEY_UP = new RemotePipiCommand(18, "up", "up", null);
        public static RemotePipiCommand CMD_KEY_MUTE = new RemotePipiCommand(19, "mute", "f8", new String[]{"volumemute"});
        public static RemotePipiCommand CMD_KEY_LEFT = new RemotePipiCommand(20, "left", "left", null);
        public static RemotePipiCommand CMD_KEY_RETURN = new RemotePipiCommand(21, "enter", "return", null);
        public static RemotePipiCommand CMD_KEY_RIGHT = new RemotePipiCommand(22, "right", "right", null);
        public static RemotePipiCommand CMD_KEY_BACK = new RemotePipiCommand(23, "back", "back", null);
        public static RemotePipiCommand CMD_KEY_DOWN = new RemotePipiCommand(24, "down", "down", null);
        public static RemotePipiCommand CMD_KEY_GUIDE = new RemotePipiCommand(25, "guide", "f1", null);
        public static RemotePipiCommand CMD_KEY_CHUP = new RemotePipiCommand(26, "chup", "pageup", null);
        public static RemotePipiCommand CMD_KEY_CHDWN = new RemotePipiCommand(27, "chdown", "next", null);
        public static RemotePipiCommand CMD_KEY_RECORD = new RemotePipiCommand(28, "record", "control, r", null);
        public static RemotePipiCommand CMD_KEY_REPEAT = new RemotePipiCommand(29, "repeat", "control, shift, r", null);//new String[] { "Back" });
        public static RemotePipiCommand CMD_KEY_PLAY = new RemotePipiCommand(30, "play", "control, shift, p", null);
        public static RemotePipiCommand CMD_KEY_PAUSE = new RemotePipiCommand(31, "pause", "control, p", new String[]{"mediaplaypause"});
        public static RemotePipiCommand CMD_KEY_STOP = new RemotePipiCommand(32, "stop", "control, s", new String[]{"mediastop", "numpad0", "insert"});
        public static RemotePipiCommand CMD_KEY_REWIND = new RemotePipiCommand(33, "rewind", "control, shift, d", null);
        public static RemotePipiCommand CMD_KEY_FFWD = new RemotePipiCommand(34, "ffwd", "control, shift, f", null);
        public static RemotePipiCommand CMD_KEY_PREVIOUS = new RemotePipiCommand(35, "previous", "control, b", new String[] { "mediaprevioustrack", "delete", "decimal" });
        public static RemotePipiCommand CMD_KEY_NEXT = new RemotePipiCommand(36, "next", "control, f", new String[]{"medianexttrack"});
        public static RemotePipiCommand CMD_KEY_VOLUMEDOWN = new RemotePipiCommand(37, "volumedown", "volumedown", new String[] { "subtract" });
        public static RemotePipiCommand CMD_KEY_VOLUMEUP = new RemotePipiCommand(38, "volumeup", "volumeup", new String[]{"add"});
        public static RemotePipiCommand CMD_KEY_FULLSCREEN = new RemotePipiCommand(38, "fullscreen", "alt, return", null);

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
                if ((remoteCmdList[i].CommandCode == cmdCode))
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
			MLog.Log(null, "RemotePipi null command code=" + cmdCode);
            return null;
        }
        public static RemotePipiCommand GetCommandByName(String cmdName)
        {
            for (int i = 0; i < remoteCmdList.Length; i++)
            {
                if ((remoteCmdList[i].CommandName == cmdName))
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

        public static bool IsControlKey(string key)
        {
            return GetCommandByCode(key) != null;
        }
    }
}
