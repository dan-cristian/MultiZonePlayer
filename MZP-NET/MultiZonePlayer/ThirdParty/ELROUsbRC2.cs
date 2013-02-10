using System;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace USB_RC2
{
	class ELROUsbRC2
	{
		private IntPtr _DeviceNotificationHandle;
		private int _HIDHandle;
		private bool _MyDeviceDetected;
		private DeviceManagement _MyDeviceManagement = new DeviceManagement();
		private string _MyDevicePathName;
		private Hid _MyHID = new Hid();
		private int _ReadHandle;
		private int _WriteHandle;
		private string HIDUsage;
		private byte anbancnt = 0;
		private string str_databasepathname;

		private DataSet DS_RFSpec;

		public ELROUsbRC2()
		{
			this.DatabasePathName = Application.StartupPath + @"\AnBanLight.mdb";
			this.DS_RFSpec_ini();
		}

		public string RFOn(int devicecode)
		{
			//this.btn_Normal_on.Enabled = false;
			this.HidDevice_Open();
			string text = this.HidDevice_GetStatus();
			if (text == "ok")
			{
				text = this.RFController_SendCMD(devicecode, "ON");
			}
			MultiZonePlayer.MLog.Log(this, "ON result="+text);
			//this.btn_Normal_on.Enabled = true;
			this.HidDevice_Close();
			return text;
		}

		public string RFOff(int devicecode)
		{
			//this.btn_Normal_on.Enabled = false;
			this.HidDevice_Open();
			string text = this.HidDevice_GetStatus();
			if (text == "ok")
			{
				text = this.RFController_SendCMD(devicecode, "OFF");
			}
			MultiZonePlayer.MLog.Log(this, "OFF result=" + text);
			//this.btn_Normal_on.Enabled = true;
			this.HidDevice_Close();
			return text;
		}

		private string HidDevice_GetStatus()
		{
			byte[] buf = new byte[8];
			buf[0] = 6;
			buf[1] = 1;
			string str = this.SendOutputReports(buf);
			if (str != "ok")
			{
				this.HidDevice_Close();
				str = this.HidDevice_Open();
			}
			return str;
		}

		private void HidDevice_Close()
		{
			FileIOApiDeclarations.CloseHandle(this._ReadHandle);
			FileIOApiDeclarations.CloseHandle(this._WriteHandle);
			FileIOApiDeclarations.CloseHandle(this._HIDHandle);
		}

		private string HidDevice_Open()
		{
			short num = 0x4d9;
			short num2 = 0x1357;
			string[] devicePathName = new string[0x80];
			int index = 0;
			int num4 = 0;
			FileIOApiDeclarations.SECURITY_ATTRIBUTES structure = new FileIOApiDeclarations.SECURITY_ATTRIBUTES();
			bool flag3 = false;
			string str = "error";
			try
			{
				Guid empty = Guid.Empty;
				this._MyDeviceDetected = false;
				structure.lpSecurityDescriptor = 0;
				structure.bInheritHandle = Convert.ToInt32(true);
				structure.nLength = Marshal.SizeOf(structure);
				HidApiDeclarations.HidD_GetHidGuid(ref empty);
				if (this._MyDeviceManagement.FindDeviceFromGuid(empty, ref devicePathName))
				{
					index = 0;
					do
					{
						this._HIDHandle = FileIOApiDeclarations.CreateFile(devicePathName[index], 0, 3, ref structure, 3, 0, 0);
						if (this._HIDHandle != -1)
						{
							this._MyHID.DeviceAttributes.Size = Marshal.SizeOf(this._MyHID.DeviceAttributes);
							if (HidApiDeclarations.HidD_GetAttributes(this._HIDHandle, ref this._MyHID.DeviceAttributes) != 0)
							{
								if ((this._MyHID.DeviceAttributes.VendorID == num) & (this._MyHID.DeviceAttributes.ProductID == num2))
								{
									this._MyDeviceDetected = true;
									str = "ok";
									this._MyDevicePathName = devicePathName[index];
								}
								else
								{
									this._MyDeviceDetected = false;
									str = "Can not find the usb wireless ReomteController!";
									num4 = FileIOApiDeclarations.CloseHandle(this._HIDHandle);
								}
							}
							else
							{
								this._MyDeviceDetected = false;
								str = "Error in filling HIDD_ATTRIBUTES structure";
								num4 = FileIOApiDeclarations.CloseHandle(this._HIDHandle);
							}
						}
						index++;
					}
					while (!(this._MyDeviceDetected | (index == devicePathName.Length)));
				}
				if (this._MyDeviceDetected)
				{
					flag3 = this._MyDeviceManagement.RegisterForDeviceNotifications(this._MyDevicePathName, 
						//base.Handle, 
						MultiZonePlayer.ControlCenter.Instance.Handle,

						empty, ref this._DeviceNotificationHandle);
					this._MyHID.Capabilities = this._MyHID.GetDeviceCapabilities(this._HIDHandle);
					if (flag3)
					{
						this.HIDUsage = this._MyHID.GetHIDUsage(this._MyHID.Capabilities);
						this._ReadHandle = FileIOApiDeclarations.CreateFile(this._MyDevicePathName, 0x80000000, 3, ref structure, 3, 0x40000000, 0);
						if (this._ReadHandle == -1)
						{
							str = "The usb device is " + this.HIDUsage + "，this device is used by OS !";
						}
						else
						{
							this._WriteHandle = FileIOApiDeclarations.CreateFile(this._MyDevicePathName, 0x40000000, 3, ref structure, 3, 0, 0);
						}
						this._MyHID.FlushQueue(this._ReadHandle);
					}
					return str;
				}
				return "Can not find the usb device!";
			}
			catch (Exception exception)
			{
				return exception.Message.ToString();
			}
		}

		public string DatabasePathName
		{
			get
			{
				return this.str_databasepathname;
			}
			set
			{
				this.str_databasepathname = value;
			}
		}

		private void DS_RFSpec_ini()
		{
			this.DS_RFSpec = new DataSet();
			try
			{
				string connectionString = "Provider=Microsoft.Jet.OLEDB.4.0; Data Source=" + this.DatabasePathName;
				string selectCommandText = " SELECT *  FROM RFSpec  order by ID ";
				OleDbConnection selectConnection = new OleDbConnection(connectionString);
				new OleDbDataAdapter(selectCommandText, selectConnection).Fill(this.DS_RFSpec);
				selectConnection.Close();
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.Message, "Info", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}

		private string RFController_SendCMD(int _DeviceCode, string _cmd)
		{
			string str = this.RFController_SendCMD_AnBan(_DeviceCode, _cmd);
			if ((_cmd == "ON") || (_cmd == "OFF"))
			{
				if (str == "ok")
				{
					str = this.RFController_SendCMD_UK(_DeviceCode, _cmd);
				}
				if (str == "ok")
				{
					str = this.RFController_SendCMD_GER(_DeviceCode, _cmd);
				}
			}
			return str;
		}

		private string RFController_SendCMD_AnBan(int _DeviceCode, string _cmd)
		{
			byte[,] rFCMDBuf = new byte[4, 8];
			this.AnBan_BuilCMD(ref rFCMDBuf, _DeviceCode, _cmd);
			string str = this.RFController_DownLoadCMD(rFCMDBuf);
			if (str == "ok")
			{
				str = this.RFController_Exec();
			}
			return str;
		}

		private string RFController_SendCMD_GER(int _DeviceCode, string _cmd)
		{
			byte[,] rFCMDBuf = new byte[4, 8];
			this.GER_BuildCMD(ref rFCMDBuf, _DeviceCode, _cmd);
			string str = this.RFController_DownLoadCMD(rFCMDBuf);
			if (str == "ok")
			{
				str = this.RFController_Exec();
			}
			return str;
		}

		private string RFController_SendCMD_UK(int _DeviceCode, string _cmd)
		{
			byte[,] rFCMDBuf = new byte[4, 8];
			this.UK_BuildCMD(ref rFCMDBuf, _DeviceCode, _cmd);
			string str = this.RFController_DownLoadCMD(rFCMDBuf);
			if (str == "ok")
			{
				str = this.RFController_Exec();
			}
			return str;
		}

		private string RFController_Exec()
		{
			byte[] buf = new byte[8];
			buf[0] = 5;
			return this.SendOutputReports(buf);
		}

		private string RFController_DownLoadCMD(byte[,] RFCMDBuf)
		{
			string str = "ok";
			byte[] buf = new byte[8];
			for (int i = 0; (i < 4) && (str == "ok"); i++)
			{
				for (int j = 0; j < 8; j++)
				{
					buf[j] = RFCMDBuf[i, j];
				}
				str = this.SendOutputReports(buf);
			}
			return str;
		}

		private void RFSpecs_Find(ref byte[,] RFCMDBuf, string SpecName)
		{
			int num = -1;
			/*if (this.DS_RFSpec.Tables[0].Rows.Count > 0)
			{
				for (int i = 0; i < this.DS_RFSpec.Tables[0].Rows.Count; i++)
				{
					if (this.DS_RFSpec.Tables[0].Rows[i]["RFName"].ToString() == SpecName)
					{
						num = i;
					}
				}
				if (num >= 0)
				{
					RFCMDBuf[0, 0] = 1;
					RFCMDBuf[1, 0] = 2;
					int num3 = int.Parse(this.DS_RFSpec.Tables[0].Rows[num]["StartBit_HTime"].ToString()) / 10;
					RFCMDBuf[0, 1] = (byte)((num3 >> 8) & 0xff);
					RFCMDBuf[0, 2] = (byte)(num3 & 0xff);
					num3 = int.Parse(this.DS_RFSpec.Tables[0].Rows[num]["StartBit_LTime"].ToString()) / 10;
					RFCMDBuf[0, 3] = (byte)((num3 >> 8) & 0xff);
					RFCMDBuf[0, 4] = (byte)(num3 & 0xff);
					num3 = int.Parse(this.DS_RFSpec.Tables[0].Rows[num]["EndBit_HTime"].ToString()) / 10;
					RFCMDBuf[0, 5] = (byte)((num3 >> 8) & 0xff);
					RFCMDBuf[0, 6] = (byte)(num3 & 0xff);
					num3 = int.Parse(this.DS_RFSpec.Tables[0].Rows[num]["EndBit_LTime"].ToString()) / 10;
					RFCMDBuf[0, 7] = (byte)((num3 >> 8) & 0xff);
					RFCMDBuf[1, 1] = (byte)(num3 & 0xff);
					num3 = int.Parse(this.DS_RFSpec.Tables[0].Rows[num]["DataBit0_HTime"].ToString()) / 10;
					RFCMDBuf[1, 2] = (byte)(num3 & 0xff);
					num3 = int.Parse(this.DS_RFSpec.Tables[0].Rows[num]["DataBit0_LTime"].ToString()) / 10;
					RFCMDBuf[1, 3] = (byte)(num3 & 0xff);
					num3 = int.Parse(this.DS_RFSpec.Tables[0].Rows[num]["DataBit1_HTime"].ToString()) / 10;
					RFCMDBuf[1, 4] = (byte)(num3 & 0xff);
					num3 = int.Parse(this.DS_RFSpec.Tables[0].Rows[num]["DataBit1_LTime"].ToString()) / 10;
					RFCMDBuf[1, 5] = (byte)(num3 & 0xff);
					num3 = int.Parse(this.DS_RFSpec.Tables[0].Rows[num]["DataBit_Count"].ToString());
					RFCMDBuf[1, 6] = (byte)(num3 & 0xff);
					num3 = int.Parse(this.DS_RFSpec.Tables[0].Rows[num]["Frame_Count"].ToString());
					RFCMDBuf[1, 7] = (byte)(num3 & 0xff);
				}
			}
			 */
		}

		private string SendOutputReports(byte[] buf)
		{
			try
			{
				if ((this._ReadHandle != -1) && (this._WriteHandle != -1))
				{
					if (this._MyHID.Capabilities.OutputReportByteLength > 0)
					{
						byte[] reportBuffer = new byte[this._MyHID.Capabilities.OutputReportByteLength];
						reportBuffer[0] = 0;
						for (int i = 0; i < 8; i++)
						{
							reportBuffer[i + 1] = buf[i];
						}
						Hid.OutputReportViaControlTransfer transfer = new Hid.OutputReportViaControlTransfer();
						if (transfer.Write(reportBuffer, this._WriteHandle))
						{
							return "ok";
						}
						return "Output report error ! ";
					}
					return "The device can not send output report!";
				}
				return "The device is mouse or keyboard ! ";
			}
			catch (Exception exception)
			{
				return exception.Message;
			}
		}

		private void UK_BuildCMD(ref byte[,] RFCMDBuf, int _DeviceCode, string _cmd)
		{
			this.RFSpecs_Find(ref RFCMDBuf, "UK");
			this.UK_RFData_Build(ref RFCMDBuf, _DeviceCode, _cmd);
		}

		private ushort UK_GetAddr(int _DeviceCode)
		{
			int num;
			int[] numArray = new int[8];
			for (num = 0; num < 8; num++)
			{
				numArray[num] = _DeviceCode % 3;
				_DeviceCode /= 3;
			}
			for (num = 0; num < 8; num++)
			{
				switch (numArray[num])
				{
					case 0:
						numArray[num] = 0;
						break;

					case 1:
						numArray[num] = 3;
						break;

					case 2:
						numArray[num] = 1;
						break;
				}
			}
			ushort num2 = 0;
			for (num = 7; num >= 0; num--)
			{
				num2 = (ushort)(num2 << 2);
				num2 = (ushort)(num2 | ((ushort)numArray[num]));
			}
			return num2;
		}

		private void UK_RFData_Build(ref byte[,] RFCMDBuf, int _DeviceCode, string _cmd)
		{
			RFCMDBuf[2, 0] = 3;
			RFCMDBuf[3, 0] = 4;
			ushort num = this.UK_GetAddr(_DeviceCode);
			byte num2 = 20;
			if (_cmd == "ON")
			{
				num2 = (byte)(num2 | 1);
			}
			RFCMDBuf[2, 1] = (byte)((num >> 8) & 0xff);
			RFCMDBuf[2, 2] = (byte)(num & 0xff);
			RFCMDBuf[2, 3] = num2;
			RFCMDBuf[2, 4] = 0;
			RFCMDBuf[2, 5] = 0;
			RFCMDBuf[2, 6] = 0;
			RFCMDBuf[2, 7] = 0;
			RFCMDBuf[3, 1] = 0;
			RFCMDBuf[3, 2] = 0;
			RFCMDBuf[3, 3] = 0;
			RFCMDBuf[3, 4] = 0;
			RFCMDBuf[3, 5] = 0;
			RFCMDBuf[3, 6] = 0;
			RFCMDBuf[3, 7] = 0;
		}

		private void AnBan_BuilCMD(ref byte[,] RFCMDBuf, int _DeviceCode, string _cmd)
		{
			this.RFSpecs_Find(ref RFCMDBuf, "AnBan");
			this.AnBan_RFData_Build(ref RFCMDBuf, _DeviceCode, _cmd);
		}

		private void AnBan_RFData_Build(ref byte[,] RFCMDBuf, int _DeviceCode, string _cmd)
		{
			int[] numArray = new int[] { 0x609, 0x306, 0x803, 0xa08, 10, 0x200, 0xc02, 0x40c, 0xe04, 0x70e, 0x507, 0x105, 0xf01, 0xb0f, 0xd0b, 0x90d };
			RFCMDBuf[2, 0] = 3;
			RFCMDBuf[3, 0] = 4;
			this.anbancnt = (byte)(this.anbancnt + 1);
			int[] numArray2 = new int[10];
			numArray2[0] = 1;
			numArray2[1] = (this.anbancnt << 2) & 15;
			if (_cmd != "OFF")
			{
				numArray2[1] |= 2;
			}
			numArray2[2] = _DeviceCode & 15;
			numArray2[3] = (_DeviceCode >> 4) & 15;
			numArray2[4] = (_DeviceCode >> 8) & 15;
			numArray2[5] = (_DeviceCode >> 12) & 15;
			if ((_cmd == "ON") || (_cmd == "OFF"))
			{
				numArray2[6] = 0;
			}
			else
			{
				numArray2[6] = byte.Parse(_cmd.Substring(0, 1)) - 1;
				numArray2[6] |= 8;
			}
			byte[] buffer = new byte[7];
			int index = numArray2[0];
			buffer[0] = (byte)(numArray[index] >> 8);
			index = numArray2[1] ^ buffer[0];
			buffer[1] = (byte)(numArray[index] >> 8);
			index = numArray2[2] ^ buffer[1];
			buffer[2] = (byte)(numArray[index] >> 8);
			index = numArray2[3] ^ buffer[2];
			buffer[3] = (byte)(numArray[index] >> 8);
			index = numArray2[4] ^ buffer[3];
			buffer[4] = (byte)(numArray[index] >> 8);
			index = numArray2[5] ^ buffer[4];
			buffer[5] = (byte)(numArray[index] >> 8);
			buffer[6] = (byte)numArray2[6];
			byte[] buffer2 = new byte[7];
			index = buffer[0];
			buffer2[0] = (byte)(numArray[index] & 0xff);
			index = buffer[1] ^ buffer2[0];
			buffer2[1] = (byte)(numArray[index] & 0xff);
			index = buffer[2] ^ buffer2[1];
			buffer2[2] = (byte)(numArray[index] & 0xff);
			index = buffer[3] ^ buffer2[2];
			buffer2[3] = (byte)(numArray[index] & 0xff);
			index = buffer[4] ^ buffer2[3];
			buffer2[4] = (byte)(numArray[index] & 0xff);
			index = buffer[5] ^ buffer2[4];
			buffer2[5] = (byte)(numArray[index] & 0xff);
			buffer2[6] = (byte)(buffer[6] ^ 9);
			int num2 = ((((((buffer2[6] << 0x18) | (buffer2[5] << 20)) | (buffer2[4] << 0x10)) | (buffer2[3] << 12)) | (buffer2[2] << 8)) | (buffer2[1] << 4)) | buffer2[0];
			num2 = (num2 >> 2) | ((num2 & 3) << 0x1a);
			RFCMDBuf[2, 1] = (byte)(num2 >> 20);
			RFCMDBuf[2, 2] = (byte)(num2 >> 12);
			RFCMDBuf[2, 3] = (byte)(num2 >> 4);
			RFCMDBuf[2, 4] = (byte)(num2 << 4);
			RFCMDBuf[2, 5] = 0;
			RFCMDBuf[2, 6] = 0;
			RFCMDBuf[2, 7] = 0;
			RFCMDBuf[3, 1] = 0;
			RFCMDBuf[3, 2] = 0;
			RFCMDBuf[3, 3] = 0;
			RFCMDBuf[3, 4] = 0;
			RFCMDBuf[3, 5] = 0;
			RFCMDBuf[3, 6] = 0;
			RFCMDBuf[3, 7] = 0;
		}

		private void GER_BuildCMD(ref byte[,] RFCMDBuf, int _DeviceCode, string _cmd)
		{
			this.RFSpecs_Find(ref RFCMDBuf, "GER");
			this.GER_RFData_Build(ref RFCMDBuf, _DeviceCode, _cmd);
		}

		private void GER_RFData_Build(ref byte[,] RFCMDBuf, int _DeviceCode, string _cmd)
		{
			int num;
			int[] numArray = new int[] { 7, 11, 13, 14, 0x13, 0x15, 0x16, 0x19, 0x1a, 0x1c, 3, 5, 6, 9, 10, 12 };
			RFCMDBuf[2, 0] = 3;
			RFCMDBuf[3, 0] = 4;
			byte[] buffer = new byte[] { 0, (byte)((_DeviceCode >> 8) & 0xff), (byte)(_DeviceCode & 0xff), 1 };
			if (_cmd == "ON")
			{
				buffer[3] = (byte)(buffer[3] | 0x10);
			}
			byte[] buffer2 = new byte[] { (byte)(buffer[0] >> 4), (byte)(buffer[0] & 15), (byte)(buffer[1] >> 4), (byte)(buffer[1] & 15), (byte)(buffer[2] >> 4), (byte)(buffer[2] & 15), (byte)(buffer[3] >> 4), (byte)(buffer[3] & 15) };
			byte[] buffer3 = new byte[8];
			for (num = 0; num < 8; num++)
			{
				buffer3[num] = (byte)numArray[buffer2[num]];
			}
			for (num = 0; num < 8; num++)
			{
				buffer3[num] = (byte)(buffer3[num] | 0x40);
			}
			for (num = 0; num < 8; num++)
			{
				buffer3[num] = (byte)(buffer3[num] & 0x7f);
			}
			buffer3[0] = (byte)(buffer3[0] | 0x80);
			ulong num2 = 0L;
			num2 = buffer3[0];
			for (num = 1; num < 8; num++)
			{
				num2 = (num2 << 7) | buffer3[num];
			}
			num2 = num2 << 7;
			buffer3[0] = (byte)(num2 >> 0x38);
			buffer3[1] = (byte)(num2 >> 0x30);
			buffer3[2] = (byte)(num2 >> 40);
			buffer3[3] = (byte)(num2 >> 0x20);
			buffer3[4] = (byte)(num2 >> 0x18);
			buffer3[5] = (byte)(num2 >> 0x10);
			buffer3[6] = (byte)(num2 >> 8);
			buffer3[7] = (byte)num2;
			RFCMDBuf[2, 1] = buffer3[0];
			RFCMDBuf[2, 2] = buffer3[1];
			RFCMDBuf[2, 3] = buffer3[2];
			RFCMDBuf[2, 4] = buffer3[3];
			RFCMDBuf[2, 5] = buffer3[4];
			RFCMDBuf[2, 6] = buffer3[5];
			RFCMDBuf[2, 7] = buffer3[6];
			RFCMDBuf[3, 1] = buffer3[7];
			RFCMDBuf[3, 2] = 0;
			RFCMDBuf[3, 3] = 0;
			RFCMDBuf[3, 4] = 0;
			RFCMDBuf[3, 5] = 0;
			RFCMDBuf[3, 6] = 0;
			RFCMDBuf[3, 7] = 0;
		}
	}
}
