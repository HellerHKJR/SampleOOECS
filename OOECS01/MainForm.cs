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
using Acs.Eib.ECS;
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
        IEcsMaintenanceMessageHandler, IEcsRecipeMessageHandler, IEcsTerminalMessageHandler,
        IEcsVariablesMessageHandler
    {
        private ApplicationRoot root = null;
        private ECSInterface ecsInterface = null;
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
            Console.WriteLine("Control State Changed at {0}, from {1} to {2}", interfaceName, previousState, currentState);
        }

        public void CommunicationStateChange(string interfaceName, bool previousState, bool currentState)
        {
            Console.WriteLine("Communication State Changed at {0}, from {1} to {2}", interfaceName, previousState, currentState);
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
        
        /// <summary>
        /// S7F3 or S7F23 Reply Send Recipe from Host
        /// </summary>
        /// <param name="recipe"></param>
        /// <returns></returns>
        public CommandReply SendRecipeToECS(Recipe recipe)
        {
            //[[[S7F3 or S7F23 From Host]]]
            //recipe.RecipeBody object[] 형식을 확인하여 모든 데이타 취출
            //recipe.RecipeName

            if (recipe.RecipeName == "aaaa")
                return new CommandReply(null, 0L, true);    //ACK
            
            throw new CustomAckCodeException("", 1);    //NACK

        }

        /// <summary>
        /// S7F23 Formatted Recipe Send to Host
        /// </summary>
        /// <param name="strTmpPPID"></param>
        /// <param name="strPPID"></param>
        /// <param name="cCode"></param>
        public void PPFormattedSend(string strTmpPPID, string strPPID, string cCode)
        {
            //S7F23 To Host
            //임시 생성되어 있는 Recipe를 갖고 진행을 한 이후 Host Return 값에 따라 원복 또는 덮어쓰기 해야되므로 TmpPPID를 사용함.
            // ReturnRecipeFormatted 의 내용을 수정하여 다른 Function을 만들어서 대체해야 channel 별 구별이 가능할 것 같음.
            //RecipeArg arg = RecipeArg.LookupTempName(strTmpPPID);
            //string[] paramList = arg.ReturnRecipeFormatted();
            //if( arg != null )
            //{ }
            //else
            //{ logging }

            object data = null;

            data = new object[] { new object[] { 1, new object[] { 12, 5.6, "ert" } } };

            Recipe recipe = new Recipe(strPPID, data, Recipe.FORMATTED_RECIPE);

            recipeService.SendRecipeToEIB(recipe);
        }

        /// <summary>
        /// S7F3 Unformatted Recipe send to Host
        /// </summary>
        /// <param name="strPPID"></param>
        public void PPUnformattedSend(string strPPID)
        {
            //S7F3 To Host
            object data = null;

            ////RecipeArg arg = RecipeArg.LookupName(strPPID);
            ////if (arg != null)
            ////{
            ////    data = arg.ReturnRecipeUnformatted();
            ////}

            Recipe recipe = new Recipe(strPPID, Recipe.UNFORMATTED_RECIPE);
            recipe.RecipeBody = data;

            recipeService.SendRecipeToEIB(recipe);

        }
        
        /// <summary>
        /// S7F5, S7F25 PP Request from Host
        /// </summary>
        /// <param name="recipe"></param>
        /// <returns></returns>
        public Recipe RequestRecipeFromECS(Recipe recipe)
        {
            // S7F5 or S7F25 From Host
            if( recipe.RecipeFormat == Recipe.FORMATTED_RECIPE )
            {
                //recipe.RecipeBody = ;
            }
            else
            {
                //recipe.RecipeBody = ;
            }

            return recipe;

        }

        /// <summary>
        /// S7F5 Unformatted Recipe Request to Host
        /// </summary>
        /// <param name="strPPID"></param>
        public void PPUnformattedRequest(string strPPID)
        {
            //throw new Exception();
            Recipe recipe = new Recipe(strPPID, Recipe.UNFORMATTED_RECIPE);
            recipeService.RequestRecipeFromEIB(recipe);
        }

        /// <summary>
        /// S7F17 Delete Recipe List from Host
        /// </summary>
        /// <param name="recipeNames"></param>
        /// <returns></returns>
        public CommandReply DeleteRecipe(string[] recipeNames)
        {
            // S7F17 From Host
            List<string> rtnPPs = new List<string>();
            List<string> deletePPs = new List<string>();
            bool isFullySuccess = false;

            // check fully has or not
            if (recipeNames == null || recipeNames.Count() == 0) isFullySuccess = false;
            else
            {
                foreach (string strRcp in recipeNames)
                {
                    ////RecipeArg arg = RecipeArg.LookupName(strRcp);
                    ////if (arg != null)
                    ////{
                    ////    deletePPs.Add(arg.RecipeFile.FullName);
                    ////    rtnPPs.Add(strRcp);
                    ////    isFullySuccess = true;
                    ////}
                    ////else
                    ////{
                    ////    isFullySuccess = false;
                    ////    break;
                    ////}
                }
            }

            //Delete
            if (isFullySuccess)
            {
                string subDeleteCheck = string.Empty;
                foreach (string tmp in deletePPs)
                {
                    ////if (!RecipeArg.DeleteRecipe(tmp))
                    ////{
                    ////    subDeleteCheck = tmp;
                    ////    break;
                    ////}
                }

                if (string.IsNullOrEmpty(subDeleteCheck))
                    return new CommandReply(null, 0L, true);
                else
                {
                    ////UseLog.Log(UseLog.LogCategory.Event, "PPDelete - Some of recipe did not delete because of access violation : {0}", subDeleteCheck);
                    ////throw new CustomAckCodeException("", (int)SECSEnumValues.Ackc7.PermissionNotGranted);
                    throw new CustomAckCodeException("", 1);
                }
            }
            else
            {
                ////UseLog.Log(UseLog.LogCategory.Event, "PPDelete - Some or all of recipe did not found.");
                ////throw new CustomAckCodeException("", (int)SECSEnumValues.Ackc7.PPIDNotFound);
                throw new CustomAckCodeException("", 4);
            }
        }

        /// <summary>
        /// S7F19 Reqeust Recipe List from Host
        /// </summary>
        /// <returns></returns>
        public string[] ListRecipe()
        {
            //S7F19 From Host
            List<string> recipeNames = new List<string>();
            recipeNames.Add("AAA0");
            recipeNames.Add("AAA1");
            recipeNames.Add("AAA2");
            recipeNames.Add("AAA3");
            return recipeNames.ToArray();
        }

        /// <summary>
        /// S7F33 Recipe Availability Check from Host
        /// </summary>
        /// <param name="recipeId"></param>
        /// <returns></returns>
        public RecipeAvailability CheckRecipeAvailabilityFromHost(string recipeId)
        {
            //S7F33 PP Available Request
            //insert formatted length and unformatted length/
            //will not use

            throw new NotImplementedException();
            //return new RecipeAvailability(recipeId, 100, 200);
        }
        #endregion

        #region IEcsTerminalMessageHandler
        public void TerminalDisplay(EcsTerminalService terminal, string[] messages)
        {
            string text = "";
            foreach (string msg in messages)
                text += msg + "\n";
            ////OnChangeControl?.Invoke(this, new ControlArg("TerminalMessage", text));

            Console.WriteLine("Terminal Message : " + text);

            terminalService.MessageRecognized(); //Event Trigger for MessageRecognition
        }

        public void SendTerminalMessage(long lTid, string terminalMsg)
        {
            try
            {
                EcsTerminalService ts = terminalService.TerminalServiceStore.GetTerminalService(lTid.ToString());
                terminalService.TerminalRequest(ts, terminalMsg);
                
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error sending terminal request.\n" + ex.Message);
            }
        }

        #endregion

        #region IEcsVariablesMessageHandler
        public object GetVariable(EcsVariable variable)
        {
            //Heller Not Use
            //source option is SourcedByECS
            throw new NotImplementedException();
        }

        public object[] GetVariables(EcsVariable[] variables)
        {
            //Heller Not Use
            // S1F3, S2F13
            //source option is SourcedByECS
            throw new NotImplementedException();
        }

        public void SetVariable(EcsVariable variable, object variableValue)
        {
            //Heller Not Use
            //source option is SourcedByECS
            //Heller Not Use: S2F15 New Equipment Constant Send 2 (If value OK and source option is SourcedByECS)
            throw new NotImplementedException();
        }

        public void SetVariables(EcsVariable[] variables, object[] variableValues)
        {
            //Heller Not Use
            //source option is NofifyEcsOnSet
            //Heller Not Use: S2F15 New Equipment Constant Send 3 (If source option is NofifyEcsOnSet)
            throw new NotImplementedException();
        }

        public void VerifyVariableValue(EcsVariable variable, object newValue)
        {
            //TODO
            //S2F15 New Equipment Constant Send 1
            // Heller EC Option : verifyValue : True, sourceOption : SuppliedByECS
            // c.f. Heller SV Option : verifyValue : False, sourceOption : SuppliedByECS
            // c.f. Heller DV Option : verifyValue : False, sourceOption : SuppliedByECS
            // S2F15에 대해 List 구성에 불구하고, 개별 VID 별로 발생한다.

            //Hashtable tmpTable = new Hashtable();
            //for (long i = 0; i < nCount; i++)
            //{
            //    tmpTable.Add(pnEcids[i], psVals[i]);
            //}
            //// 설비측 EC 변경 시도 : try to change EC
            //OnChangeControl?.Invoke(this, new ControlArg("ChangeRequestECFromHost", tmpTable, new object[] { nMsgId }));

            //RangeError
            SubError err = (SubError)new IllegalValueSubError("Ecs Error");
            //NoError
            //SubError err = (SubError)new NoErrorParameterSubError("", new ObjParameterData[] { new ObjParameterData("result", newValue) });
            //현재 변경 불가시
            //SubError err = (SubError)new CannotPerformNowSubError("Ecs Error");
            if (err != null)
                throw ErrorFactory.CreateException(err.ErrorCode, err.Message, true);

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
                btnStart2.Enabled = false;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Failed to Start - {0}", ex);
                if (root != null) root.CoreLoaded -= Root_CoreLoaded;

                ApplicationRoot.Reset();
            }
        }

        private void btnStart2_Click(object sender, EventArgs e)
        {
            try
            {
                root = ApplicationRoot.Singleton;
                root.CoreLoaded += Root_CoreLoaded;
                new MainFactory().Make(root, @"C:\Personal\Projects\EIB\HellerEIB\HellerEIB_TEST.xml", true);
                ecsServerNameTextBox.Text = "ECSInterface_Oven";
                btnStart.Enabled = false;
                btnStart2.Enabled = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to Start - {0}", ex);
                if (root != null) root.CoreLoaded -= Root_CoreLoaded;

                ApplicationRoot.Reset();
            }
        }

        public void UnloadThis()
        {
            maintenanceService.ShutdownApplication(false, false);
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
                
                // Update form controls
                if (alarmService != null)
                    alarmButton.Enabled = true;

                //alarmService.AlarmStore.GetAlarm("aaa").TargetObject.Enabled

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

        public void SendAlarm(string alarmID, bool isSet)
        {
            // Show alarm form and get Alarm ID and Alarm State
            try
            {                
                EcsAlarm alarm = alarmService.AlarmStore.GetAlarm(alarmID);
                if (alarm != null)
                {
                    //alarmService.AlarmChangedVariable(alarm, isSet ? 1 : 0, null, null);
                    alarmService.AlarmChanged(alarm, isSet ? 1 : 0);
                    Console.WriteLine("Alarm successfully sent to EIB.");
                }
                else
                {
                    throw new Exception(string.Format("Alarm '{0}' does not exist.", alarmID));
                }
            }
            catch
            {
                Console.WriteLine("Unable to send alarm to EIB.");
            }
        }


        public void SendEvent(string sCEID, object dvList)
        {
            //TODO : 인수 중 object 부분을 DV 들의 리스트로 대체할 것.
            EcsEvent evt = eventsService.EventStore.GetEvent(sCEID);

            //TODO : 실제 인수로 할 것...
            string[] kkk = new string[] { "21000", "username", "softrev" };

            bool isAllOK = true;
            List<EcsVariable> ecsVariables = new List<EcsVariable>();
            for( int i = 0; i < kkk.Length; i++ )
            {
                EcsVariable v = variableService.VariableStore.GetVariable(kkk[i]);
                if( v == null )
                {
                    Console.WriteLine("Variable " + kkk[i] + " does not exist.");
                    isAllOK = false;
                }
                else
                {
                    ecsVariables.Add(v);
                }
            }

            if (isAllOK)
            {
                object[] ecsValues = new object[] { };
                eventsService.TriggerEvent(evt, ecsVariables.ToArray(), ecsVariables.Count == 0 ? new object[] { } : ecsValues);
            }
        }

        public void SetVariables(object vList)
        {
            //vList를 실제의 내용으로 변환 할 것.
            string[] kkk = new string[] { "21000", "username", "softrev" };
            List<EcsVariable> ecsVariables = new List<EcsVariable>();
            List<object> ecsValues = new List<object>();
            for (int i = 0; i < kkk.Length; i++)
            {
                EcsVariable v = variableService.VariableStore.GetVariable(kkk[i]);
                if (v == null)
                {
                    //just skip
                    Console.WriteLine("Variable " + kkk[i] + " does not exist.");                    
                }
                else
                {
                    ecsVariables.Add(v);
                    ecsValues.Add("SSSSSSSS");
                }
            }

            if( ecsVariables.Count > 0 )
            {
                variableService.VariablesChanged(ecsVariables.ToArray(), ecsValues.ToArray());
            }
            
        }

        public void SetEquipConst(object vList)
        {            
            string[] kkk = new string[] { "21000", "username", "softrev" };
            for (int i = 0; i < kkk.Length; i++)
            {
                EcsVariable v = variableService.VariableStore.GetVariable(kkk[i]);
                if (v == null)
                {
                    //just skip
                    Console.WriteLine("Variable " + kkk[i] + " does not exist.");
                }
                else
                {
                    variableService.VariableChanged(v, "SSSSS");
                    variableService.OperatorEquipmentConstantChanged(v, false);
                }
            }
        }
        
        static bool isSet = false;
        private void alarmButton_Click(object sender, EventArgs e)
        {
            var kkk = alarmService.GetAlarmsStates(new string[] { "testAlarm" });
            EcsAlarmStateInformation ttt = kkk[0] as EcsAlarmStateInformation;
            
            SendAlarm("testAlarm", !isSet);

            isSet = !isSet;
        }

        private void eventButton_Click(object sender, EventArgs e)
        {

        }

        
    }
}
