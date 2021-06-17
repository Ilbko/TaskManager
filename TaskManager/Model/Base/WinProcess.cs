using System.Drawing;

namespace TaskManager.Model.Base
{
    public class WinProcess
    {
        public WinProcess() { }
        public Image Picture { get; set; }
        public string Name { get; set; }
        public int Id { get; set; }
        public string UserName { get; set; }
        public long Memory { get; set; }
    }
}
