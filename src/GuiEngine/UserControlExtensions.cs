using System;
using System.Windows.Forms;
using UnhedderEngine;

namespace GuiEngine
{
    public static class UserControlExtensions
    {
        public static bool TryInvoke(this UserControl control, Action action)
        {
            try
            {
                control.Invoke(action);
            }
            catch (Exception ex1)
            {
                try
                {
                    action();
                }
                catch (Exception ex2)
                {
                    CoreLogger.Write(ex1);
                    CoreLogger.Write(ex2);
                    return false;
                }
            }
            return true;
        }
    }
}
