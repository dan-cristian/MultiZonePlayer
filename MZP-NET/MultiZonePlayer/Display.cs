using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Reflection;
using System.Threading;

namespace MultiZonePlayer {
	public abstract class Display : SerialBase {
		protected ZoneDetails m_zoneDetails;

		public enum InputTypeEnum {
			AV,
			HDMI1,
			HDMI2,
			Unknown
		};

		public ZoneDetails ZoneDetails {
			get { return m_zoneDetails; }
		}

		public enum DisplayTypeEnum {
			LGTV,
			XBMC //not ok here, xbmc not a serial display
		}

		//later,for various displays
		public virtual Boolean IsOn { get; set; }

		public virtual Boolean IsOnCached { get; set; }

		//for various displays
		public virtual int DisplayOffCount { get; set; }
		public virtual InputTypeEnum InputTypeCached { get; set; }
		public abstract InputTypeEnum InputType { get; set; }
	}

	public class DisplayLGTV : Display {
		public enum LGCommandsEnum {
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

		public enum InputCodesEnum {
			[Description("AV 1")] x_20,
			[Description("HDMI 1")] x_a0,
			[Description("HDMI 1a")] x_90,
			[Description("HDMI 2")] x_91
		};

		private int DISPLAY_ID = 0;
		private string m_input, m_connection;
		private bool m_isOnValue = false;
		private InputTypeEnum m_inputType = InputTypeEnum.Unknown;
		private int m_volumeLevel = -1;
		private int m_stdtimeout = 4000;
		private DateTime m_lastTVResponsive = DateTime.MinValue;
		private DateTime m_lastTVPowerOn = DateTime.MinValue;
		private int m_displayOffCount = -1;

		public DisplayLGTV(String connection, ZoneDetails p_zoneDetails) {
			m_connection = connection;
			Reinitialise();
			m_zoneDetails = p_zoneDetails;
			m_zoneDetails.ZoneState = ZoneState.NotStarted;
		}

		~DisplayLGTV() {
			Disconnect();
		}

		public override int DisplayOffCount {
			get { return m_displayOffCount; }
		}

		public void Reinitialise() {
			Initialise("9600", "None", "One", "8", m_connection, CommunicationManager.TransmissionType.Text);
		}

		public override String SendCommand(Enum cmd, String value) {
			String command = cmd.ToString().Split('_')[1] + " " + DISPLAY_ID.ToString() + " " + value;
			String result = WriteCommand(command, 1, m_stdtimeout);
			return result;
		}

		private bool SendCommandCheckResult(Enum cmd, String value, out String resultValue) {
			String response = SendCommand(cmd, value);
			bool wasOk = response.ToLower().Contains("ok");
			if (response.Length > 3) {
				resultValue = response.Substring(response.Length - 3, 2);
			}
			else {
				resultValue = "";
			}
			return wasOk;
		}

		public override String GetCommandStatus(Enum cmd) {
			String result;
			result = SendCommand(cmd, "FF").ToLower();

			if (result.Contains("ok")) {
				result = result.Substring(result.IndexOf("ok") + 2, 2);
				m_lastTVResponsive = DateTime.Now;
			}
			else {
				//m_isOnValue = false;
				m_displayOffCount++;
				//TODO might be a blocker when tv is off
				//Reinitialise();
			}

			return result;
		}

		protected override void ReceiveSerialResponse(string response) {
			MLog.Log(this, "Received unexpected LGTV serial response: " + response);
			//m_lastMessageResponse = response.ToLower();
			//m_lastOperationWasOK = m_lastMessageResponse.Contains("ok");
		}

		public void NextInput() {
			InputCodesEnum currentInput, nextInput = InputCodesEnum.x_20;
			bool found = false;
			String inputEnumCode = "x_" + m_input;

			if (Enum.IsDefined(typeof (InputCodesEnum), inputEnumCode)) {
				currentInput = (InputCodesEnum) Enum.Parse(typeof (InputCodesEnum), inputEnumCode);
				foreach (InputCodesEnum en in Enum.GetValues(typeof (InputCodesEnum))) {
					if (found) {
						nextInput = en;
						break;
					}

					if (en.Equals(currentInput)) {
						found = true;
					}
				}

				MLog.Log(this, "Next input current=" + currentInput + " next=" + nextInput);

				Input = nextInput.ToString().Substring(2);
			}
			else {
				MLog.Log(this, "Error on next input, input code not defined=" + inputEnumCode);
			}
		}

		public InputTypeEnum InputTypeNative {
			get {
				String res = Input;
				switch (res) {
					case "20":
						return InputTypeEnum.AV;
					case "90":
					case "a0":
						return InputTypeEnum.HDMI1;
					case "91":
						return InputTypeEnum.HDMI2;
					default: {
						MLog.Log(this, "Unknown input type=" + res);
						return InputTypeEnum.Unknown;
					}
				}
			}
		}

		public String Input {
			get {
				String res = GetCommandStatus(DisplayLGTV.LGCommandsEnum.INPUTSELECT_xb);
				m_input = res;
				return res;
			}
			set {
				String inputValue;
				if (SendCommandCheckResult(DisplayLGTV.LGCommandsEnum.INPUTSELECT_xb, value, out inputValue)) {
					m_input = inputValue;
				}
			}
		}

		public override InputTypeEnum InputTypeCached {
			get { return m_inputType; }
		}

		public override InputTypeEnum InputType {
			get {
				InputTypeEnum result = InputTypeNative;
				if (result != m_inputType) {
					MLog.Log(this, "Display TV Input changed to " + result);
				}
				m_inputType = result;
				return result;
			}
			set {
				switch (value) {
					case InputTypeEnum.HDMI1:
						Input = "90";
						break;
					case InputTypeEnum.HDMI2:
						Input = "91";
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

		public String InputCached {
			get { return m_input; }
		}

		public override bool IsOnCached {
			get { return m_isOnValue; }
		}

		public override Boolean IsOn {
			get {
				Boolean result = GetCommandStatus(DisplayLGTV.LGCommandsEnum.POWER_ka) == "01";
				if (m_isOnValue != result) {
					MLog.Log(this, "TV state changed now On is " + result);
				}
				if (result) {
					m_lastTVPowerOn = DateTime.Now;
				}
				else {
					m_displayOffCount++;
				}
				m_isOnValue = result;
				return result;
			}
			set {
				String cmdval = value ? "01" : "00";
				String isOnValue;
				//if (
				SendCommandCheckResult(DisplayLGTV.LGCommandsEnum.POWER_ka, cmdval, out isOnValue);
				//m_isOnValue = isOnValue=="01";
			}
		}

		public int VolumeLevel {
			get {
				String volumeHex = GetCommandStatus(DisplayLGTV.LGCommandsEnum.VOLUMECONTROL_kf);
				return ReturnVolume(volumeHex);
			}
			set {
				String volumeValue;
				if (SendCommandCheckResult(DisplayLGTV.LGCommandsEnum.VOLUMECONTROL_kf, value.ToString("X"), out volumeValue)) //hex
				{
					m_volumeLevel = ReturnVolume(volumeValue);
				}
			}
		}

		private int ReturnVolume(String volumeHex) {
			int vol;
			if (Int32.TryParse(volumeHex, System.Globalization.NumberStyles.HexNumber,
				System.Globalization.CultureInfo.InvariantCulture, out vol)) {
				return vol;
			}
			else {
				return -1;
			}
		}

		public Boolean VolumeMute {
			get { return GetCommandStatus(DisplayLGTV.LGCommandsEnum.VOLUMEMUTE_ke) == "01"; }
			set { SendCommand(DisplayLGTV.LGCommandsEnum.VOLUMEMUTE_ke, "01"); }
		}
	}
}