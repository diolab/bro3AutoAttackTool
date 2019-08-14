using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bro3AutoAttackTool
{
    public partial class SelectZengunBusyo : Form
    {
        public struct zgb
        {
            public bool chk;
            public int id;
            public string name;
        }

        public List<zgb> zgblist = new List<zgb>();

        public SelectZengunBusyo()
        {
            InitializeComponent();
        }

        private void SelectZengunBusyo_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
        }

        public void setgrid()
        {
            dgv.Rows.Clear();
            foreach (zgb _b in zgblist)
            {
                dgv.Rows.Add();
                dgv.Rows[dgv.Rows.Count - 2].Cells[1].Value = _b.id.ToString();
                dgv.Rows[dgv.Rows.Count - 2].Cells[2].Value = _b.name;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            zgblist.Clear();
            foreach (DataGridViewRow row in dgv.Rows)
            {
                zgb _b = new zgb();
                try
                {
                    _b.chk = false;
                    _b.id = 0;
                    _b.name = string.Empty;

                    if (row.Cells[0].Value != null) { _b.chk = (bool)row.Cells[0].Value; }
                    if (row.Cells[1].Value != null) {
                        int.TryParse(row.Cells[1].Value.ToString(), out _b.id);
                    }
                    if (row.Cells[2].Value != null) { _b.name = row.Cells[2].Value.ToString().Trim(); }
                }
                catch (Exception ex) { Console.WriteLine(ex.Message); }

                if (_b.id + _b.name.Length > 0)
                {
                    zgblist.Add(_b);
                }
            }
        }

        private void dgv_CellStyleChanged(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dgv_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dgv_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (0 == e.ColumnIndex)
            {
                for (int i = 0; i < dgv.Rows.Count; i++)
                {
                    if (i != e.RowIndex)
                    {
                        dgv.Rows[i].Cells[0].Value = false;
                    }
                }
            }
        }


    }
}
