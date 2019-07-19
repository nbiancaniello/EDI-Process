using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class _Default : System.Web.UI.Page
{
    SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.AppSettings.Get("ConnInfo"));
    SqlDataAdapter DataAdapter = new SqlDataAdapter();
    SqlCommand SelectCommand = new SqlCommand();
    SqlCommand SqlQuery = new SqlCommand();

    protected void Page_Load(object sender, EventArgs e)
    {
        if (Page.IsPostBack)
        {
            ddlOH_UOM_From.Items.Add("Choose an UOM...");
            ddlOH_UOM_From.Items.Add("KG");
            ddlOH_UOM_From.Items.Add("G");
            ddlOH_UOM_From.Items.Add("EA");
            ddlOH_UOM_From.Items.Add("PCS");
            ddlOH_UOM_From.Items.Add("TON");
            ddlOH_UOM_From.Items.Add("TONNE");
            ddlOH_UOM_From.Items.Add("LBS");
        }

        if (hdfPartNo.Value == "")
        {
            hdfPartNo.Value = Request.QueryString.GetValues("part").GetValue(0).ToString();
        }
        populate();
    }

    protected void ddlOH_UOM_From_SelectedIndexChanged(object sender, EventArgs e)
    {
        try
        {
            ddlOH_UOM_To.Items.Clear();
            txtValue_To.Text = "";
            if (ddlOH_UOM_From.SelectedIndex != 0)
            {
                foreach (var item in ddlOH_UOM_From.Items)
                {
                    if (item.ToString() != ddlOH_UOM_From.SelectedValue.ToString())
                    {
                        ddlOH_UOM_To.Items.Add(item.ToString());
                    }
                }
                ddlOH_UOM_To.Enabled = true;
            }
            else
            {
                ddlOH_UOM_To.Enabled = false;
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }


    protected void btnAddItem_Click(object sender, EventArgs e)
    {
        try
        {
            using (SqlConnection con = new SqlConnection(System.Configuration.ConfigurationManager.AppSettings.Get("ConnInfo")))
            using (SqlCommand cmd = new SqlCommand(@"INSERT INTO MEASURES_CONVERSION VALUES(@part,@fromUOM,@toUOM,@tovalue)"))
            {
                cmd.Connection = con;
                //cmd.Parameters.AddWithValue("@part", Request.QueryString.GetValues("part").GetValue(0).ToString());
                cmd.Parameters.AddWithValue("@part", hdfPartNo.Value);
                cmd.Parameters.AddWithValue("@fromUOM", ddlOH_UOM_From.Text);
                cmd.Parameters.AddWithValue("@toUOM", ddlOH_UOM_To.Text);
                cmd.Parameters.AddWithValue("@tovalue", txtValue_To.Text);


                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }
            populate();
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    protected void populate()
    {
        try
        {
            using (SqlConnection con = new SqlConnection(System.Configuration.ConfigurationManager.AppSettings.Get("ConnInfo")))
            using (SqlCommand cmd = new SqlCommand(@"SELECT * FROM MEASURES_CONVERSION WHERE FK_PART_NO = @PART_NO"))
            {
                DataTable Table = new DataTable();
                SqlDataAdapter DataAdapter = new SqlDataAdapter();
                DataAdapter.SelectCommand = cmd;

                cmd.Connection = con;
                //cmd.Parameters.AddWithValue("@PART_NO", Request.QueryString.GetValues("part").GetValue(0).ToString());
                cmd.Parameters.AddWithValue("@PART_NO", hdfPartNo.Value);

                con.Open();

                Table.Clear();
                DataAdapter.Fill(Table);

                rptDSPItems.DataSource = Table;
                rptDSPItems.DataBind();

                con.Close();
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }

    }

    protected void ddlOH_UOM_SelectedIndexChanged(object sender, EventArgs e)
    {
        //string axiomPart = Request.QueryString.GetValues("part").GetValue(0).ToString();
        string axiomPart = hdfPartNo.Value;
        string fromUOM = ddlOH_UOM_From.Text;
        string toUOM = ddlOH_UOM_To.Text;

        txtValue_To.Text = "1";//ShippingFunctions.UOM_Convert(axiomPart, fromUOM, toUOM, "1");

        if (txtValue_To.Text != null)
        {
            txtValue_To.Enabled = false;
        }
        else
        {
            txtValue_To.Enabled = true;
        }

    }

    protected void rptDSPItems_ItemCommand(object source, RepeaterCommandEventArgs e)
    {
        if (e.CommandName == "Del")
        {
            using (SqlConnection con = new SqlConnection(System.Configuration.ConfigurationManager.AppSettings.Get("ConnInfo")))
            using (SqlCommand cmd = new SqlCommand(@"DELETE FROM MEASURES_CONVERSION WHERE MEASURE_ID = @MEASURE_ID"))
            {
                cmd.Connection = con;
                cmd.Parameters.AddWithValue("@MEASURE_ID", e.CommandArgument);

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }
            populate();
        }
    }
}