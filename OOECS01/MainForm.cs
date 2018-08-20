using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Acs.Common;
using Acs.Common.Extensions;
using Acs.Eib.EibCore.Interface.ECS;
//using Acs.Common.DataType;
//using Acs.Common.Errors;
using Acs.Framework;
using Acs.Eib.EibCore;
using Acs.Eib.EibFactory;


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

        public MainForm()
        {
            InitializeComponent();
        }
        
        #region IEcsClockMessageHandler
        public object[] GetTSClockAttributes(string[] attributes)
        {
            throw new NotImplementedException();
        }

        public void SetClock(Acs.Common.DataType.BaseDate dateTime)
        {
            throw new NotImplementedException();
        }

        public void SetTSClockAttributes(string[] attributes, object[] values)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region IEcsCommandMessageHandler
        public CommandReply ExecuteCommandAsynchronous(EcsCommand command, string[] parameterNames, object[] parameterValues, long closure, int transactionId)
        {
            throw new NotImplementedException();
        }

        public CommandReply ExecuteCommandSynchronous(EcsCommand command, string[] parameterNames, object[] parameterValues, long closure)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region IEcsCustomMessageHandler
        public object RequestReplyFromECS(string messageName, object dataStructure)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public void ConfigurationCompleted()
        {
            throw new NotImplementedException();
        }

        public void ServerStarted()
        {
            throw new NotImplementedException();
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
    }
}
