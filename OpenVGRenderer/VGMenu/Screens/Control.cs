using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VGMenu.Screens
{
    public abstract class Control : IDisposable
    {

        public Control()
        {
            visible = true;
        }

        public abstract void Dispose();

        private bool visible;

        public virtual bool Visible
        {
            get { return visible; }
            set { visible = value; }
        }


        /// <summary>
        /// Occurs when the control is deactivated
        /// </summary>
        public virtual void OnLeave()
        {

        }

        /// <summary>
        /// Occurs when the control is active and receives a command
        /// </summary>
        /// <param name="key"></param>
        public virtual void OnCommand(ConsoleKey key)
        {

        }
        /// <summary>
        /// Occurs when the control gets focus
        /// </summary>
        public virtual void OnEnter()
        {

        }


        public virtual void Refresh()
        {
            
        }
    }
}
