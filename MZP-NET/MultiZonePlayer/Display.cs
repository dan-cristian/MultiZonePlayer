using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Reflection;


using System.Threading;

namespace MultiZonePlayer
{
    
    public abstract class Display:SerialBase
    {
        protected Metadata.ZoneDetails m_zoneDetails;
        public enum InputTypeEnum {AV, HDMI, Unknown };

        public Metadata.ZoneDetails ZoneDetails
        {
            get { return m_zoneDetails; }
            
        }

        public enum DisplayTypeEnum
        {
            LGTV, XBMC//not ok here, xbmc not a serial display
        }
        //later,for various displays
        public virtual Boolean IsOn
        {
            get;
            set;
        }

        
        public abstract InputTypeEnum InputType
        {
            get;
            set;
        }
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
        
        
        public enum InputCodesEnum
        {
            [Description("AV 1")] x_20,
            [Description("HDMI 1")] x_a0,
            [Description("HDMI 1a")] x_90
        };
        private int DISPLAY_ID=0;
        private string m_input;
        private int m_stdtimeout = 5000;
        

        public DisplayLGTV(String connection, Metadata.ZoneDetails p_zoneDetails)
        {
            Initialise("9600", "None", "One", "8", connection);
            m_zoneDetails = p_zoneDetails;
        }

        ~DisplayLGTV()
        {
            Disconnect();
        }

        public override String SendCommand(Enum cmd, String value)
        {
            String command = cmd.ToString().Split('_')[1] + " " + DISPLAY_ID.ToString() + " " + value;
            String result = WriteCommand(command, 1, m_stdtimeout);
            return result;
        }

        public override String GetCommandStatus(Enum cmd)
        {
            String result;
            result = SendCommand(cmd, "FF").ToLower();

            if (result.Contains("ok"))
            {
                result = result.Substring(result.IndexOf("ok") + 2, 2);
            }
            else
            {
                //MLog.Log(this, "Getstatus TV returned error, msg=" + result);
            }

            return result;
        }

        protected override void ReceiveSerialResponse(string response)
        {
            MLog.Log(this, "Received unexpected LGTV serial response: " + response);
            //m_lastMessageResponse = response.ToLower();
            //m_lastOperationWasOK = m_lastMessageResponse.Contains("ok");
        }

        public void NextInput()
        {
            InputCodesEnum currentInput, nextInput = InputCodesEnum.x_20;
            bool found = false;
            String inputEnumCode = "x_" + m_input;
            
            if (Enum.IsDefined(typeof(InputCodesEnum), inputEnumCode))
            {
                currentInput = (InputCodesEnum)Enum.Parse(typeof(InputCodesEnum), inputEnumCode);
                foreach (InputCodesEnum en in Enum.GetValues(typeof(InputCodesEnum)))
                {
                    if (found)
                    {
                        nextInput = en;
                        break;
                    }

                    if (en.Equals(currentInput)) found = true;
                }

                MLog.Log(this, "Next input current=" + currentInput + " next=" + nextInput);

                Input = nextInput.ToString().Substring(2);
            }
            else
                MLog.Log(this, "Error on next input, input code not defined=" + inputEnumCode);
        }

        public InputTypeEnum InputTypeNative
        {
            get
            {
                String res=Input;
                switch (res)
                {
                    case "20":
                        return InputTypeEnum.AV;
                    case "90":
                    case "a0":
                        return InputTypeEnum.HDMI;
                    default:
                        {
                            MLog.Log(this, "Unknown input type="+res);
                            return InputTypeEnum.Unknown;
                        }
                }
            }
        }

        public String Input
        {
            get
            {
                String res = GetCommandStatus(DisplayLGTV.LGCommandsEnum.INPUTSELECT_xb);
                m_input = res;
                return res;
            }
            set
            {
                SendCommand(DisplayLGTV.LGCommandsEnum.INPUTSELECT_xb, value);
            }
        }

        public override InputTypeEnum InputType
        {
            get
            {
                return InputTypeNative;
            }
            set
            {
                switch (value)
                {
                    case InputTypeEnum.HDMI:
                        Input = "90";
                        break;
                    case InputTypeEnum.AV:
                        Input = "20";
                        break;
                    default:
                        MLog.Log(this, "Unknown inputype value on set, val=" + value.ToString());
                        break;
                }
            }
        }

        public String InputCached
        {
            get
            { return m_input; }
        }

        public override Boolean IsOn
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
