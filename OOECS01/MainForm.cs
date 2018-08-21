using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Windows.Forms;
using Acs.Common;
using Acs.Common.Extensions;
using Acs.Eib.EibCore.Interface.ECS;
using Acs.Common.DataType;
using Acs.Common.Errors;
using Acs.Framework;
using Acs.Eib.EibCore;
using Acs.Eib.EibFactory;
using System.Threading;


namespace OOECS01
{
    public partial class MainForm : Form, IEcsClockMessageHandler, IEcsCommandMessageHandler, IEcsCustomMessageHandler,
        IEcsMaintenanceMessageHandler, IEcsRecipeMessageHandler, IEcsWaferMapMessageHandler, IEcsTerminalMessageHandler,
        IEcsVariablesMessageHandler, IEcsMessageValidationHandler
    {
        private ApplicationRoot root = null;
        private ECSInterface ecsInterface = null;
        private IEcs200mmServices ecs200mmServices = null;
        private IEcsAlarmsService alarmService = null;
        private IEcsClockService clockService = null;
        private IEcsCommandsService commandService = null;
        private IEcsCustomMessageService customMessageService = null;
        private IEcsE116EptService eptService = null;
        private IEcsEventsService eventsService = null;
        private IEcsMaintenanceService maintenanceService = null;
        private IEcsRecipeService recipeService = null;
        private IEcsTerminalServices terminalService = null;
        private IEcsVariablesService variableService = null;
        private IEcsMessageValidationService messageValidationService = null;

        public event EventHandler<ControlArg> OnChangeControl;

        public MainForm()
        {
            InitializeComponent();
        }
        
        #region IEcsClockMessageHandler
        public object[] GetTSClockAttributes(string[] attributes)
        {
            throw new NotImplementedException();
            //sample also not implemented
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SYSTEMTIME
        {
            public short wYear;
            public short wMonth;
            public short wDayOfWeek;
            public short wDay;
            public short wHour;
            public short wMinute;
            public short wSecond;
            public short wMilliseconds;
        }
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetSystemTime(ref SYSTEMTIME st);
        public void SetClock(BaseDate dateTime)
        {
            //H->E S2F17 Date and Time Request
            //For Eqp request clock, use clockService.SetClockFromSource()            
            Console.WriteLine("[EIB]Time Reply From Host: {0}", dateTime.LocalDateTime);

            try
            {
                //string timeFormat = UseXMLConfig.GetValueXPath("//SecsConfig/HostTimeFormat", strConfigPath);
                //bool timeZoneUTC = "true".Equals(UseXMLConfig.GetAttrXPath("//SecsConfig/HostTimeFormat", "useUTC0", strConfigPath));
                string timeFormat = "yyyyMMddHHmmsscc";
                bool timeZoneUTC = false;

                int yearLength = timeFormat.Count(f => f == 'y');
                int ccLength = timeFormat.Count(f => f == 'c');

                DateTime tempDateTime;

                if (timeZoneUTC) tempDateTime = dateTime.LocalDateTime;
                else tempDateTime = dateTime.GMTDateTime;

                SYSTEMTIME st = new SYSTEMTIME();
                // All of these must be short
                st.wYear = (short)tempDateTime.Year;
                st.wMonth = (short)tempDateTime.Month;
                st.wDay = (short)tempDateTime.Day;
                st.wHour = (short)tempDateTime.Hour;
                st.wMinute = (short)tempDateTime.Minute;
                st.wSecond = (short)tempDateTime.Second;

                // invoke the SetSystemTime method now
                if (!SetSystemTime(ref st))
                    Console.WriteLine("To change system time, Run as administrator");
                //UseLog.Log(UseLog.LogCategory.Event, "To change system time, Run as administrator");
            }
            catch
            {
                Console.WriteLine("Invalid Time Format. Please Check again.");
            }

        }

        public void SetTSClockAttributes(string[] attributes, object[] values)
        {
            throw new NotImplementedException();
            //sample also not implemented
        }
        #endregion

        #region IEcsCommandMessageHandler
        public CommandReply ExecuteCommandSynchronous(EcsCommand commandName, string[] parameterNames, object[] parameterValues, long closure)
        {
            throw new NotImplementedException();
        }
        public CommandReply ExecuteCommandAsynchronous(EcsCommand command, string[] parameterNames, object[] parameterValues, long closure, int transactionId)
        {
            ObjParameterData[] replyParams = new ObjParameterData[parameterNames.Length];
            for (int i = 0; i < parameterNames.Length; i++)
                replyParams[i] = new ObjParameterData(parameterNames[i], parameterValues[i]);
            
            HostRemoteCommandArg arg = new HostRemoteCommandArg(transactionId, 2, 41, closure, command.CommandName);
            for (int i = 0; i < parameterNames.Length; i++)
                arg.SetCPVal(parameterNames[i], parameterValues[i].ToString());

            OnChangeControl?.BeginInvoke(this, new ControlArg("RemoteCommand", arg, replyParams), null, null);

            ////Test
            //arg.SetHcAck(1);
            //RspRemoteCommandToHost(arg, replyParams);
            ////Test

            return null;
        }
        public void RspRemoteCommandToHost(HostRemoteCommandArg arg, ObjParameterData[] replyParams)
        {
            if( arg.HcAck != 0 )
                this.commandService.CommandFailed(new CustomAckCodeException("", Convert.ToInt32(arg.HcAck)), Convert.ToInt32(arg.nObjectID));
            else
                this.commandService.CommandAcknowledge(new CommandReply(replyParams, arg.nSB, true), Convert.ToInt32(arg.nObjectID));
        }

        #endregion

        #region IEcsCustomMessageHandler
        public object RequestReplyFromECS(string messageName, object dataStructure)
        {
            Console.WriteLine("{0}:{1}", messageName, (object[])dataStructure);
            
            return dataStructure;
        }

        public void SendCustomMessage()
        {
            customMessageService.SendCustomMessageToEIB("TestCusomMessageFromECS",
                new object[]
                {
                    "test",
                    "test2",
                    1,
                    (short)2,
                    (uint)4,
                    (byte)1
                }
                , false);
        }


        #endregion

        #region IEcsMaintenanceMessageHandler
        public void ControlStateChange(string interfaceName, byte previousState, byte currentState)
        {
            throw new NotImplementedException();
        }

        public void CommunicationStateChange(string interfaceName, bool previousState, bool currentState)
        {
            throw new NotImplementedException();
        }

        public void InterfacesLoaded()
        {
            Console.WriteLine("Interfaces Loaded!!!");
        }

        public void ConfigurationCompleted()
        {
            Console.WriteLine("Configuration Completed!!!");
        }

        public void ServerStarted()
        {
            Console.WriteLine("Server Started!!!");
        }
        #endregion

        #region IEcsRecipeMessageHandler
        public CommandReply SendRecipeToECS(Recipe recipe)
        {
            throw new NotImplementedException();
        }

        public Recipe RequestRecipeFromECS(Recipe recipe)
        {
            throw new NotImplementedException();
        }

        public CommandReply DeleteRecipe(string[] recipeNames)
        {
            throw new NotImplementedException();
        }

        public string[] ListRecipe()
        {
            throw new NotImplementedException();
        }

        public RecipeAvailability CheckRecipeAvailabilityFromHost(string recipeId)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region IEcsWaferMapMessageHandler
        public void MapErrorToTool(short maper, short datlc)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region IEcsTerminalMessageHandler
        public void TerminalDisplay(EcsTerminalService terminal, string[] messages)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region IEcsVariablesMessageHandler
        public object GetVariable(EcsVariable variable)
        {
            throw new NotImplementedException();
        }

        public object[] GetVariables(EcsVariable[] variables)
        {
            throw new NotImplementedException();
        }

        public void SetVariable(EcsVariable variable, object variableValue)
        {
            throw new NotImplementedException();
        }

        public void SetVariables(EcsVariable[] variables, object[] variableValues)
        {
            throw new NotImplementedException();
        }

        public void VerifyVariableValue(EcsVariable variable, object newValue)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region IEcsMessageValidationHandler
        public object RequestValidationReplyFromECS(string messageName, object dataStructure)
        {
            throw new NotImplementedException();
        }

        public object RequestReplyValidationFromECS(string messageName, object request, object reply)
        {
            throw new NotImplementedException();
        }

        #endregion

        public override object InitializeLifetimeService()
        {
            return null;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                root = ApplicationRoot.Singleton;                
                root.CoreLoaded += Root_CoreLoaded;
                new MainFactory().Make(root, txtToolModel.Text, true);
                btnStart.Enabled = false;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Failed to Start - {0}", ex);
                if (root != null) root.CoreLoaded -= Root_CoreLoaded;

                ApplicationRoot.Reset();
            }
        }

        private void Root_CoreLoaded(object sender, EventArgs e)
        {
            ICommInterface commInterface = root.GetInterface(ecsServerNameTextBox.Text);
            if (commInterface == null)
            {
                Console.WriteLine("No Interface named " + ecsServerNameTextBox.Text + " exists within the tool model");
            }
            else if (!(commInterface is ECSInterface))
            {
                Console.WriteLine("The Interface " + ecsServerNameTextBox.Text + " is not an ECS Interface");
            }
            else
            {
                ecsServerNameTextBox.Enabled = false;
                Text = "Sample OO ECS - Connected";

                ecsInterface = (ECSInterface)commInterface;
                ecs200mmServices = ecsInterface.Ecs200mmServices;
                alarmService = ecsInterface.AlarmsService;
                clockService = ecsInterface.ClockService;
                if (clockService != null)
                    clockService.MessageHandler = this;
                commandService = ecsInterface.CommandsService;
                if (commandService != null)
                    commandService.MessageHandler = this;
                customMessageService = ecsInterface.CustomMessages;
                if (customMessageService != null)
                    customMessageService.MessageHandler = this;
                eptService = ecsInterface.E116EptService;
                eventsService = ecsInterface.EventsService;
                maintenanceService = ecsInterface.MaintenanceService;
                if (maintenanceService != null)
                    maintenanceService.MessageHandler = this;
                recipeService = ecsInterface.RecipeService;
                if (recipeService != null)
                    recipeService.MessageHandler = this;
                terminalService = ecsInterface.TerminalServices;
                if (terminalService != null)
                    terminalService.MessageHandler = this;
                variableService = ecsInterface.VariablesService;
                if (variableService != null)
                    variableService.MessageHandler = this;
                messageValidationService = ecsInterface.MessageValidation;
                if (messageValidationService != null)
                    messageValidationService.MessageHandler = this;

                // Update form controls
                if (alarmService != null)
                    alarmButton.Enabled = true;

                if (eventsService != null)
                    eventButton.Enabled = true;

                if (variableService != null)
                {
                    variableButton.Enabled = true;
                    btnEquipmentConstant.Enabled = true;
                    btnEibValue.Enabled = true;
                    btnTransitionStateMachine.Enabled = true;
                    btnSetStateMachineState.Enabled = true;
                }

                if (ecs200mmServices != null)
                    get200mmButton.Enabled = true;

                if (maintenanceService != null)
                {
                    btnEnableInterface.Enabled = true;
                    btnDisableInterface.Enabled = true;
                    btnShutdownEib.Enabled = true;
                    btnShutdownApplication.Enabled = true;
                    btnECSLogMessage.Enabled = true;
                }

                if (commandService != null)
                {
                    btnOperatorCommand.Enabled = true;
                }
                if (terminalService != null)
                {
                    btnTerminalRequest.Enabled = true;
                    btnMessageRecognized.Enabled = true;
                }

                if (recipeService != null)
                {
                    btnRecipeSend.Enabled = true;
                    btnRequestRecipe.Enabled = true;
                    btnRecipeSelect.Enabled = true;
                    btnRecipeChanged.Enabled = true;
                    btnRecipesSelect.Enabled = true;
                    btnDownloadFormattedRecipeVerification.Enabled = true;
                    btnDownloadUnformattedRecipeVerification.Enabled = true;
                }

                if (clockService != null)
                    setClockFromSourceButton.Enabled = true;

            }
        }

        private void setClockFromSourceButton_Click(object sender, EventArgs e)
        {
            try
            {
                clockService.SetClockFromSource();
                //MessageBox.Show("Command successfully issued.");
            }
            catch (Exception ex)
            {
                //MessageBox.Show(this, "Error sending command.\n " + ex.Message);
            }
        }
    }
}
