using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading;

namespace MultiZonePlayer
{
    
    public abstract class Display:SerialBase
    {
        //later,for various displays
    }

    public class DisplayLGTV: Display
    {
        public enum LGCommandsEnum
        {
            POWER_ka,
            ASPECTRATIO_kc,
            SCREENMUTE_kd,
            VOLUMEMUTE_ke,
            VOLUMECONTROL_kf,
            CONTRAST_kg,
            BRIGHTNESS_kh,
            COLOUR_ki,
            TINT_kj,
            SHARPNESS_kk,
            OSDSELECT_kl,
            REMOTECONTROL_km,
            TREBLE_kr,
            BASS_kt,
            COLOURTEMPERATURE_ku,
            ISMMETHOD_jp,
            ENERGYSAVING_jq,
            AUTOCONFIGURATION_ju,
            TUNECOMMAND_ma,
            PROGRAMMEADDSKIP_mb,
            KEY_mc,
            CONTROLBACKLIGHT_mg,
            INPUTSELECT_xb
        };
        public enum InputTypeEnum {AV, HDMI };
        private int DISPLAY_ID=0;
        

        public DisplayLGTV(String connection)
        {
            Initialise("9600", "None", "One", "8", connection);
        }

        public override String SendCommand(Enum cmd, String value)
        {
            String command = cmd.ToString().Split('_')[1] + " " + DISPLAY_ID.ToString() + " " + value;
            WriteCommand(command);
            return "ok";
        }

        public override String GetCommandStatus(Enum cmd)
        {
            String result="n/a";
            SendCommand(cmd, "FF");
            if (m_lastOperationWasOK)
            {
                result = m_lastMessageResponse.Substring(m_lastMessageResponse.IndexOf("ok") + 2, 2);
            }
            else
                result = "error for status:" + cmd.ToString() + " " + m_lastMessageResponse;

            return result;
        }

        public String InputType
        {
            get
            {
                String res=GetCommandStatus(DisplayLGTV.LGCommandsEnum.INPUTSELECT_xb);
                switch (res)
                {
                    case "20":
                        return InputTypeEnum.AV.ToString();
                    case "90":
                    case "0a":
                        return InputTypeEnum.HDMI.ToString();
                    default:
                        return "Unknown:" + res;
                }
            }
        }

        public Boolean IsOn
        {
            get
            {
                return GetCommandStatus(DisplayLGTV.LGCommandsEnum.POWER_ka) == "01";
            }
            set
            {
                String cmdval = value ? "01" : "00";
                SendCommand(DisplayLGTV.LGCommandsEnum.POWER_ka, cmdval);
            }
        }

        public int VolumeLevel
        {
            get
            {
                String volume = GetCommandStatus(DisplayLGTV.LGCommandsEnum.VOLUMECONTROL_kf);
                int vol;
                if (Int32.TryParse(volume, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out vol))
                    return vol;
                else
                    return -1;
            }
            set
            {
                SendCommand(DisplayLGTV.LGCommandsEnum.VOLUMECONTROL_kf, value.ToString("X"));//hex
            }
        }

        public Boolean VolumeMute
        {
            get
            {
                return GetCommandStatus(DisplayLGTV.LGCommandsEnum.VOLUMEMUTE_ke) == "01";
            }
            set
            {
                SendCommand(DisplayLGTV.LGCommandsEnum.VOLUMEMUTE_ke, "01");
            }
        }

    }
}
