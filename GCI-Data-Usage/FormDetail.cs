using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataUsage
{
    public partial class FormDetail : Form
    {
        public IEnumerable<Service> Detail
        {
            set
            {
                textBox1.Text = String.Join(Environment.NewLine, value.Select(v => v.ToString()).ToArray());
            }
        }

        public FormDetail()
        {
            InitializeComponent();
        }

        private void FormDetail_Load(object sender, EventArgs e)
        {

        }
    }
}
