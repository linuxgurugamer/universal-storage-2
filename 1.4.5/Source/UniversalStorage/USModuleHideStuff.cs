
namespace UniversalStorage
{
    public class USModuleHideStuff : PartModule
    {
        public override void OnStart(StartState state)
        {
            ModuleCommand command = part.FindModuleImplementing<ModuleCommand>();

            if (command == null)
                return;

            if (command.Events["MakeReference"] != null)
            {
                command.Events["MakeReference"].guiActive = false;
                command.Events["MakeReference"].guiActiveEditor = false;
                command.Events["MakeReference"].guiActiveUncommand = false;
                command.Events["MakeReference"].guiActiveUnfocused = false;
                command.Events["MakeReference"].active = false;
            }

            if (command.Events["RenameVessel"] != null)
            {
                command.Events["RenameVessel"].guiActive = false;
                command.Events["RenameVessel"].guiActiveEditor = false;
                command.Events["RenameVessel"].guiActiveUncommand = false;
                command.Events["RenameVessel"].guiActiveUnfocused = false;
                command.Events["RenameVessel"].active = false;
            }

            if (command.Actions["MakeReferenceToggle"] != null)
            {
                command.Actions["MakeReferenceToggle"].active = false;
            }
        }
    }
}
