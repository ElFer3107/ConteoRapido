using CoreCRUDwithORACLE.Interfaces;
using CoreCRUDwithORACLE.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;

namespace CoreCRUDwithORACLE.Servicios
{
    public class ServicioUsuario : IServicioUsuario
    {
        private readonly string _conn;
        private Comunes.Auxiliar _helper = new Comunes.Auxiliar();
        private readonly ILogger _logger;

        private string consultaUsuarios = @"SELECT US.COD_USUARIO, PR.COD_PROVINCIA, PR.NOM_PROVINCIA, US.CED_USUARIO || US.DIG_USUARIO CEDULA, US.LOG_USUARIO,
                                                US.CLA_USUARIO, US.NOM_USUARIO, RO.COD_ROL, RO.DES_ROL, US.EST_USUARIO, 
                                            US.MAI_USUARIO, US.TEL_USUARIO
                                           FROM USUARIO US, PROVINCIA PR, ROL RO
                                           WHERE US.COD_PROVINCIA = PR.COD_PROVINCIA
                                           AND US.COD_ROL = RO.COD_ROL";

        private string consultaUser = @"SELECT US.COD_USUARIO, PR.COD_PROVINCIA, PR.NOM_PROVINCIA, US.CED_USUARIO || US.DIG_USUARIO CEDULA, US.LOG_USUARIO,
                                               US.TEL_USUARIO, US.NOM_USUARIO, RO.COD_ROL, RO.DES_ROL, US.EST_USUARIO, US.MAI_USUARIO, US.CLA_USUARIO
                                           FROM USUARIO US, PROVINCIA PR, ROL RO
                                           WHERE US.COD_PROVINCIA = PR.COD_PROVINCIA
                                           AND US.COD_ROL = RO.COD_ROL
                                           AND CED_USUARIO = {0}";
        private string consultaUserx_CedMail = @"SELECT US.COD_USUARIO, PR.COD_PROVINCIA, PR.NOM_PROVINCIA, US.CED_USUARIO || US.DIG_USUARIO CEDULA, US.LOG_USUARIO,
                                               US.TEL_USUARIO, US.NOM_USUARIO, RO.COD_ROL, RO.DES_ROL, US.EST_USUARIO, US.MAI_USUARIO, US.CLA_USUARIO
                                           FROM USUARIO US, PROVINCIA PR, ROL RO
                                           WHERE US.COD_PROVINCIA = PR.COD_PROVINCIA
                                           AND US.COD_ROL = RO.COD_ROL
                                           AND  (CED_USUARIO = '{0}' or  MAI_USUARIO ='{1})'";
        private string consultaUserxMail = @"SELECT US.COD_USUARIO, PR.COD_PROVINCIA, PR.NOM_PROVINCIA, US.CED_USUARIO || US.DIG_USUARIO CEDULA, US.LOG_USUARIO,
                                               US.TEL_USUARIO, US.NOM_USUARIO, RO.COD_ROL, RO.DES_ROL, US.EST_USUARIO, US.MAI_USUARIO, US.CLA_USUARIO
                                           FROM USUARIO US, PROVINCIA PR, ROL RO
                                           WHERE US.COD_PROVINCIA = PR.COD_PROVINCIA
                                           AND US.COD_ROL = RO.COD_ROL
                                           AND MAI_USUARIO = '{0}'";

        private string consultaLogin = @"select count(*)
                                        from USUARIO
                                        where MAI_USUARIO = '{0}'
                                        and CLA_USUARIO = '{1}'";

        private string consultaUsuario = @"select COD_ROL, NOM_USUARIO, COD_PROVINCIA, EST_CLA_USUARIO, CED_USUARIO || DIG_USUARIO CEDULA, EST_USUARIO
                                        from USUARIO
                                        where MAI_USUARIO = '{0}'
                                        and CLA_USUARIO = '{1}'
                                        ";

        private string consultaCodUsuario = @"SELECT MAX(COD_USUARIO) Codigo FROM USUARIO";

        private string ingresaUsuario = @"INSERT INTO CONTEORAPIDO2021.USUARIO (COD_USUARIO,COD_ROL, LOG_USUARIO, 
                                           CLA_USUARIO, CED_USUARIO, DIG_USUARIO, NOM_USUARIO, MAI_USUARIO, EST_USUARIO, 
                                           EST_CLA_USUARIO, NIV_USUARIO, COD_PROVINCIA, PRI_LOG_USUARIO, 
                                           INT_LOGIN, TEL_USUARIO, ULT_USUARIO) 
                                           VALUES ({0}, {1}, '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', {8}, 0, 0,
                                             {9}, NULL, NULL, '{10}', NULL)";

        private string actualizaUsuario = @"UPDATE CONTEORAPIDO2021.USUARIO
                                            SET    COD_ROL         = {0},
                                                   LOG_USUARIO     = '{1}',
                                                   CED_USUARIO     = '{2}',
                                                   DIG_USUARIO     = '{3}',
                                                   NOM_USUARIO     = '{4}',
                                                   MAI_USUARIO     = '{5}',
                                                   EST_USUARIO     = '{6}',
                                                   COD_PROVINCIA   =  {7},
                                                   TEL_USUARIO     = '{8}'
                                            WHERE  COD_USUARIO     = {9}";

        private string actualizaUsuarioClave = @"UPDATE CONTEORAPIDO2021.USUARIO
                                                    SET    CLA_USUARIO     = '{0}',
                                                           EST_CLA_USUARIO = '{1}'
                                                    WHERE  CED_USUARIO     = '{2}'";

        public ServicioUsuario(IConfiguration _configuration, ILoggerFactory logger)
        {
            _conn = _configuration.GetConnectionString("OracleDBConnection");
            _logger = logger.CreateLogger<ServicioUsuario>();
        }

        public IEnumerable<Usuario> GetUsuarios(int codigoRol, int codigoProvincia)
        {

            List<Usuario> usuarios = null;

            using (OracleConnection con = new OracleConnection(_conn))
            {
                using (OracleCommand cmd = new OracleCommand())
                {
                    try
                    {
                        con.Open();
                        cmd.Connection = con;
                        cmd.CommandType = CommandType.Text;
                        switch (codigoRol)
                        {
                            case 1:
                                consultaUsuarios += " AND US.COD_ROL = 2 ";
                                break;
                            case 2:
                                consultaUsuarios += " AND US.COD_ROL in (3,5) ";
                                break;
                            case 3:
                                consultaUsuarios += " AND US.COD_ROL = 4 AND US.COD_PROVINCIA = " + codigoProvincia;
                                break;
                            case 5:
                                consultaUsuarios += " AND US.COD_ROL = 4";
                                break;
                        }

                        consultaUsuarios += " ORDER BY 1";

                        cmd.CommandText = string.Format(consultaUsuarios);

                        OracleDataReader odr = cmd.ExecuteReader();

                        if (odr.HasRows)
                        {
                            usuarios = new List<Usuario>();
                            while (odr.Read())
                            {
                                Usuario usuario = new Usuario
                                {
                                    CEDULA = Convert.ToString(odr["CEDULA"]),
                                    COD_PROVINCIA = Convert.ToInt32(odr["COD_PROVINCIA"]),
                                    PROVINCIA = Convert.ToString(odr["NOM_PROVINCIA"]),
                                    COD_ROL = Convert.ToInt32(odr["COD_ROL"]),
                                    ROL = Convert.ToString(odr["DES_ROL"]),
                                    COD_USUARIO = Convert.ToInt32(odr["COD_USUARIO"]),
                                    NOMBRE = Convert.ToString(odr["nom_usuario"]),
                                    TELEFONO = Convert.ToString(odr["TEL_USUARIO"]),
                                    MAIL = Convert.ToString(odr["MAI_USUARIO"]),
                                    LOGEO = Convert.ToString(odr["LOG_USUARIO"]),
                                    ESTADO = (Convert.ToString(odr["EST_USUARIO"]) == "1" ? true : false)
                                };
                                usuarios.Add(usuario);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        return usuarios = null;
                    }
                    finally
                    {
                        con.Close();
                        con.Dispose();
                        cmd.Dispose();
                    }

                }
            }

            return usuarios;
        }

        public Usuario GetUsuarioXMail(string iMail)
        {
            Usuario usuario = null;

            using (OracleConnection con = new OracleConnection(_conn))
            {
                using (OracleCommand cmd = new OracleCommand())
                {
                    try
                    {
                        con.Open();
                        cmd.Connection = con;
                        //cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandType = CommandType.Text;
                        //cmd.CommandText = "PKG_CONTEO_RAPIDO.CONSULTA_USUARIO";
                        cmd.CommandText = string.Format(consultaUserxMail, iMail);
                        cmd.BindByName = true;

                        OracleDataReader odr = cmd.ExecuteReader();
                        if (odr.HasRows)
                        {
                            usuario = new Usuario();
                            while (odr.Read())
                            {
                                usuario.COD_USUARIO = Convert.ToInt32(odr["COD_USUARIO"]);
                                usuario.CEDULA = Convert.ToString(odr["CEDULA"]);
                                usuario.COD_PROVINCIA = Convert.ToInt32(odr["COD_PROVINCIA"]);
                                usuario.CLAVE = Convert.ToString(odr["CLA_USUARIO"]);
                                usuario.ESTADO = (Convert.ToString(odr["EST_USUARIO"]) == "1" ? true : false);
                                usuario.NOMBRE = Convert.ToString(odr["NOM_USUARIO"]);
                                usuario.PROVINCIA = Convert.ToString(odr["NOM_PROVINCIA"]);
                                usuario.LOGEO = Convert.ToString(odr["LOG_USUARIO"]);
                                usuario.COD_ROL = Convert.ToInt32(odr["COD_ROL"]);
                                usuario.TELEFONO = Convert.ToString(odr["TEL_USUARIO"]);
                                usuario.ROL = Convert.ToString(odr["DES_ROL"]);
                                usuario.MAIL = Convert.ToString(odr["MAI_USUARIO"]);
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        //return usuario;
                    }
                    finally
                    {
                        con.Close();
                        con.Dispose();
                        cmd.Dispose();
                    }

                }
            }

            return usuario;
        }
        public Usuario GetUsuarioXcedulaMail(string iCedula,string iMail)
        {
            Usuario usuario = null;

            using (OracleConnection con = new OracleConnection(_conn))
            {
                using (OracleCommand cmd = new OracleCommand())
                {
                    try
                    {
                        con.Open();
                        cmd.Connection = con;
                        //cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandType = CommandType.Text;
                        //cmd.CommandText = "PKG_CONTEO_RAPIDO.CONSULTA_USUARIO";
                        cmd.CommandText = string.Format(consultaUserx_CedMail, iCedula.Substring(0, 9),iMail);
                        cmd.BindByName = true;

                        OracleDataReader odr = cmd.ExecuteReader();
                        if (odr.HasRows)
                        {
                            usuario = new Usuario();
                            while (odr.Read())
                            {
                                usuario.COD_USUARIO = Convert.ToInt32(odr["COD_USUARIO"]);
                                usuario.CEDULA = Convert.ToString(odr["CEDULA"]);
                                usuario.COD_PROVINCIA = Convert.ToInt32(odr["COD_PROVINCIA"]);
                                usuario.CLAVE = Convert.ToString(odr["CLA_USUARIO"]);
                                usuario.ESTADO = (Convert.ToString(odr["EST_USUARIO"]) == "1" ? true : false);
                                usuario.NOMBRE = Convert.ToString(odr["NOM_USUARIO"]);
                                usuario.PROVINCIA = Convert.ToString(odr["NOM_PROVINCIA"]);
                                usuario.LOGEO = Convert.ToString(odr["LOG_USUARIO"]);
                                usuario.COD_ROL = Convert.ToInt32(odr["COD_ROL"]);
                                usuario.TELEFONO = Convert.ToString(odr["TEL_USUARIO"]);
                                usuario.ROL = Convert.ToString(odr["DES_ROL"]);
                                usuario.MAIL = Convert.ToString(odr["MAI_USUARIO"]);
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        //return usuario;
                    }
                    finally
                    {
                        con.Close();
                        con.Dispose();
                        cmd.Dispose();
                    }

                }
            }

            return usuario;
        }
        public Usuario GetUsuario(string iCedula)
        {
            Usuario usuario = null;

            using (OracleConnection con = new OracleConnection(_conn))
            {
                using (OracleCommand cmd = new OracleCommand())
                {
                    try
                    {
                        con.Open();
                        cmd.Connection = con;
                        //cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandType = CommandType.Text;
                        //cmd.CommandText = "PKG_CONTEO_RAPIDO.CONSULTA_USUARIO";
                        cmd.CommandText = string.Format(consultaUser, iCedula.Substring(0, 9));
                        cmd.BindByName = true;

                        OracleDataReader odr = cmd.ExecuteReader();
                        if (odr.HasRows)
                        {
                            usuario = new Usuario();
                            while (odr.Read())
                            {
                                usuario.COD_USUARIO = Convert.ToInt32(odr["COD_USUARIO"]);
                                usuario.CEDULA = Convert.ToString(odr["CEDULA"]);
                                usuario.COD_PROVINCIA = Convert.ToInt32(odr["COD_PROVINCIA"]);
                                usuario.CLAVE = Convert.ToString(odr["CLA_USUARIO"]);
                                usuario.ESTADO = (Convert.ToString(odr["EST_USUARIO"]) == "1" ? true : false);
                                usuario.NOMBRE = Convert.ToString(odr["NOM_USUARIO"]);
                                usuario.PROVINCIA = Convert.ToString(odr["NOM_PROVINCIA"]);
                                usuario.LOGEO = Convert.ToString(odr["LOG_USUARIO"]);
                                usuario.COD_ROL = Convert.ToInt32(odr["COD_ROL"]);
                                usuario.TELEFONO = Convert.ToString(odr["TEL_USUARIO"]);
                                usuario.ROL = Convert.ToString(odr["DES_ROL"]);
                                usuario.MAIL = Convert.ToString(odr["MAI_USUARIO"]);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //return usuario;
                    }
                    finally
                    {
                        con.Close();
                        con.Dispose();
                        cmd.Dispose();
                    }

                }
            }

            return usuario;
        }

        public Usuario ActualizaUsuario(UsuarioResponse usuarioActualizado)
        {
            string telefono = usuarioActualizado.TELEFONO;
            Usuario usuario = null;

            //usuario = GetUsuario(usuarioActualizado.CEDULA);
            usuario = GetUsuarioXMail(usuarioActualizado.MAIL);
            if (usuario != null)
            {
                return usuario=null;
            }

            usuario = GetUsuario(usuarioActualizado.CEDULA);
            if (usuario != null)
            {
                using (OracleConnection con = new OracleConnection(_conn))
                {
                    using (OracleCommand cmd = new OracleCommand())
                    {
                        try
                        {
                            con.Open();
                            cmd.Connection = con;
                            //cmd.CommandType = CommandType.StoredProcedure;
                            cmd.CommandType = CommandType.Text;
                            //cmd.CommandText = "CONSULTA_AUTENTICACION_USUARIO";

                            cmd.CommandText = string.Format(actualizaUsuario, usuarioActualizado.CODIGO_ROL, usuarioActualizado.LOGEO,
                                                            usuarioActualizado.CEDULA, usuarioActualizado.DIGITO, usuarioActualizado.NOMBRE,
                                                            usuarioActualizado.MAIL, usuarioActualizado.ESTADO ? "1" : "0", usuarioActualizado.CODIGO_PROVINCIA,
                                                            telefono, usuarioActualizado.COD_USUARIO);

                            int odr = cmd.ExecuteNonQuery();

                            if (odr > 0)
                                usuario = GetUsuario(usuarioActualizado.CEDULA);
                            else
                                usuario = null;
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                        finally
                        {
                            con.Close();
                            con.Dispose();
                            cmd.Dispose();
                        }

                    }
                }
            }

            return usuario;
        }

        public Login GetAutenticacionUsuario(string iMail, string iPass)
        {
            Login logeo = null;
            string clave = string.Empty;

            using (OracleConnection con = new OracleConnection(_conn))
            {
                using (OracleCommand cmd = new OracleCommand())
                {
                    try
                    {
                        con.Open();
                        cmd.Connection = con;
                        //cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandType = CommandType.Text;
                        //cmd.CommandText = "CONSULTA_AUTENTICACION_USUARIO";
                        clave = _helper.EncodePassword(iPass);
                        cmd.CommandText = string.Format(consultaUsuario, iMail, clave);

                        OracleDataReader odr = cmd.ExecuteReader();

                        if (odr.HasRows)
                        {
                            while (odr.Read())
                            {
                                logeo = new Login()
                                {
                                    COD_PROVINCIA = Convert.ToInt32(odr["COD_PROVINCIA"]),
                                    COD_ROL = Convert.ToInt32(odr["COD_ROL"]),
                                    EST_CLAVE = Convert.ToInt32(odr["EST_CLA_USUARIO"]),
                                    CEDULA = Convert.ToString(odr["CEDULA"]),
                                    NOMBRE = Convert.ToString(odr["NOM_USUARIO"]),
                                    EST_USUARIO = Convert.ToInt32(odr["EST_USUARIO"])
                                };
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        return logeo;
                    }
                    finally
                    {
                        con.Close();
                        con.Dispose();
                        cmd.Dispose();
                    }

                }
            }

            return logeo;
        }

        public Usuario PutAutenticacionUsuario(string iMail, string iPass)
        {
            Usuario usuario = new Usuario();


            string clave = string.Empty;

            using (OracleConnection con = new OracleConnection(_conn))
            {
                using (OracleCommand cmd = new OracleCommand())
                {
                    try
                    {
                        con.Open();
                        cmd.Connection = con;
                        cmd.CommandType = CommandType.StoredProcedure;
                        //cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "CONSULTA_AUTENTICACION_USUARIO";
                        //cmd.CommandText = string.Format(consultaUser, iCedula);
                        cmd.BindByName = true;

                        clave = _helper.EncodePassword(iPass);
                        cmd.Parameters.Add("Return_Value", OracleDbType.RefCursor, ParameterDirection.ReturnValue);
                        cmd.Parameters.Add("I_MAIL_USUARIO", OracleDbType.Varchar2, iMail, ParameterDirection.Input);
                        cmd.Parameters.Add("I_CLAVE_USUARIO", OracleDbType.Varchar2, clave, ParameterDirection.Input);
                        cmd.Parameters.Add("o_cursor", OracleDbType.RefCursor, ParameterDirection.Output);

                        OracleDataReader odr = cmd.ExecuteReader();
                        if (odr.HasRows)
                        {

                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        con.Close();
                        con.Dispose();
                        cmd.Dispose();
                    }

                }
            }

            return usuario;
        }

        public async Task<int> IngresaUsuario(UsuarioResponse usuario)
        {
            if (usuario.CLAVE == "12345678" || usuario.CLAVE == "87654321")
            {
                _logger.LogWarning("Error: la clave no pueden ser número consecutivos" );
                return 0;
            }
                int respuesta = 0;
            using (OracleConnection con = new OracleConnection(_conn))
            {
                using (OracleCommand cmd = new OracleCommand())
                {
                    try
                    {
                        con.Open();
                        cmd.Connection = con;
                        //cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandType = CommandType.Text;
                        //cmd.CommandText = "PKG_CONTEO_RAPIDO.CONSULTA_USUARIO";
                        cmd.CommandText = string.Format(consultaCodUsuario);
                        OracleDataReader odr = cmd.ExecuteReader();
                        if (odr.HasRows)
                        {
                            while (odr.Read())
                            {
                                usuario.COD_USUARIO = Convert.ToInt32(odr["Codigo"]) + 1;
                            }
                        }
                        else
                        {
                            usuario.COD_USUARIO = 1;
                        }

                        if (usuario.MAIL.Length > 20)
                            usuario.LOGEO = usuario.MAIL.Substring(0, 20);
                        else
                            usuario.LOGEO = usuario.MAIL;

                        cmd.CommandText = string.Format(ingresaUsuario, usuario.COD_USUARIO, usuario.CODIGO_ROL, usuario.LOGEO,
                                                _helper.EncodePassword(usuario.CLAVE), usuario.CEDULA.Substring(0, 9), usuario.CEDULA.Substring(9, 1),
                                                usuario.NOMBRE, usuario.MAIL, usuario.ESTADO ? "1" : "0", usuario.CODIGO_PROVINCIA, usuario.TELEFONO);

                        respuesta = await cmd.ExecuteNonQueryAsync();
                        return respuesta;

                    }
                    catch (Exception ex)
                    {

                        _logger.LogWarning("Error: " + ex.Message);
                        return respuesta;
                    }
                    finally
                    {
                        con.Close();
                        con.Dispose();
                        cmd.Dispose();
                    }

                }
            }
        }

        public Usuario ActualizaClave(Usuario usuarioNew, int estado)
        {
            Usuario usuario = null;
            string clave = string.Empty;

            usuario = GetUsuario(usuarioNew.CEDULA);

        if (usuario != null)
            {
                clave = _helper.EncodePassword(usuarioNew.CLAVE);
                if (clave== _helper.EncodePassword("12345678") || clave == _helper.EncodePassword("87654321"))
                {
                    usuario.LOGEO = "33";
                    return usuario;
                }

                if (usuario.CLAVE == clave)
                {
                    usuario.LOGEO = "99";
                    return usuario;
                }

                using (OracleConnection con = new OracleConnection(_conn))
                {
                    using (OracleCommand cmd = new OracleCommand())
                    {
                        try
                        {
                            con.Open();
                            cmd.Connection = con;
                            cmd.CommandType = CommandType.Text;

                            cmd.CommandText = string.Format(actualizaUsuarioClave, clave, estado,
                                                            usuarioNew.CEDULA.Substring(0, 9));

                            int odr = cmd.ExecuteNonQuery();

                            if (odr == 0)
                                usuario.LOGEO = "77";
                            else
                                usuario.LOGEO = "OK";

                        }
                        catch (Exception ex)
                        {

                            usuario.LOGEO = "88";
                            return usuario;
                        }
                        finally
                        {
                            con.Close();
                            con.Dispose();
                            cmd.Dispose();
                        }

                    }
                }
            }
            return usuario;
        }
        public string EncerarBase(String Detalle, String Version, String Contr)
        {
            int validador1 = 0;
            int validador2 = 0;
            int validador3 = 0;
            string encerarBase1 = @"update acta set vot_junta=0, bla_junta=0, nul_junta=0, est_acta = 0,fec_junta=null, tipo_documento=0, nov_acta=0, cod_usuario_digitador = 0, fec_transmision = null, cod_verresultados='', fec_descarga=null,est_descarga=0";
            string encerarBase3 = @"delete from asistencia";
            string encerarBase4 = @"update  resultados set  fin_resultado=0, cod_vervotos = ''";
            string encerarBase5 = @"update configuracion set est_configuracion = 0";
            string encerarBase6 = @"insert into configuracion values ((select  max(cod_configuracion) + 1 from configuracion),'" + Detalle + "','" + Version + "',to_char(SYSDATE,'DD/MM/YYYY'),1)";

            using (OracleConnection con = new OracleConnection(_conn))
            {
                using (OracleCommand cmd = new OracleCommand())
                {
                    OracleTransaction transaction;
                    con.Open();
                    cmd.Connection = con;
                    transaction = con.BeginTransaction(IsolationLevel.ReadCommitted);
                    cmd.Transaction = transaction;

                    try
                    {
                        //cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = string.Format(encerarBase1);
                        //cmd.CommandText = "PKG_APP_MOVIL.ENCERA_BASE_DATOS";
                        //cmd.Parameters.Add("I_CEDULA", OracleDbType.Varchar2, 20).Value = cedula;
                        //cmd.Parameters.Add("O_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                        int odr = cmd.ExecuteNonQuery();

                        cmd.CommandText = encerarBase1;
                        odr = cmd.ExecuteNonQuery();

                        cmd.CommandText = encerarBase3;
                        odr = cmd.ExecuteNonQuery();


                        cmd.CommandText = encerarBase4;
                        odr = cmd.ExecuteNonQuery();


                        cmd.CommandText = encerarBase5;
                        odr = cmd.ExecuteNonQuery();

                        cmd.CommandText = encerarBase6;
                        odr = cmd.ExecuteNonQuery();
                        transaction.Commit();

                        //return 1;

                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return "0";
                    }
                    finally
                    {
                        con.Close();
                        con.Dispose();
                        cmd.Dispose();
                    }

                    encerarBase1 = "";
                    encerarBase3 = "";
                    encerarBase4 = "";
                    encerarBase1 = @"select count(*) TOTAL1  from acta where vot_junta=0 and bla_junta=0 and nul_junta=0 and est_acta = 0 and cod_usuario_digitador = 0 and est_descarga=0 and fec_transmision is null and cod_verresultados is null and fec_descarga is null";
                    encerarBase3 = @"select count(*) TOTAL2 from resultados where fin_resultado=0 and cod_vervotos is null";
                    encerarBase4 = @"select count(*) TOTAL3 from asistencia";
                    int tota1 = 0;
                    int tota2 = 0;
                    int tota3 = 0;

                    using (OracleConnection con1 = new OracleConnection(_conn))
                    {
                        using (OracleCommand cmd1 = new OracleCommand())
                        {
                            OracleTransaction transaction1;
                            con1.Open();
                            cmd1.Connection = con1;
                            transaction1 = con1.BeginTransaction(IsolationLevel.ReadCommitted);
                            cmd1.Transaction = transaction1;

                            try
                            {
                                //con1.Open();
                                //cmd1.Connection = con1;
                                //cmd.CommandType = CommandType.StoredProcedure;
                                cmd1.CommandType = CommandType.Text;
                                //cmd.CommandText = "PKG_CONTEO_RAPIDO.CONSULTA_USUARIO";
                                cmd1.CommandText = string.Format(encerarBase1);
                                cmd1.BindByName = true;

                                OracleDataReader odr = cmd1.ExecuteReader();
                                if (odr.HasRows)
                                {
                                    while (odr.Read())
                                    {
                                        tota1 = Convert.ToInt32(odr["TOTAL1"]);
                                    }

                                    //validador1 = 1;
                                }
                                else
                                {
                                    //validador1 = 0;
                                }

                                cmd1.CommandText = string.Format(encerarBase3);
                                cmd1.BindByName = true;

                                OracleDataReader odr1 = cmd1.ExecuteReader();
                                if (odr1.HasRows)
                                {
                                    while (odr1.Read())
                                    {
                                        tota2 = Convert.ToInt32(odr1["TOTAL2"]);
                                    }

                                    //validador2 = 1;
                                }
                                else
                                {
                                    //validador2 = 0;
                                }

                                cmd1.CommandText = string.Format(encerarBase4);
                                cmd1.BindByName = true;

                                OracleDataReader odr2 = cmd1.ExecuteReader();
                                if (odr2.HasRows)
                                {
                                    while (odr1.Read())
                                    {
                                        tota3 = Convert.ToInt32(odr2["TOTAL3"]);
                                    }
                                    //validador3 = 0;
                                }
                                else
                                {
                                    //validador3 = 1;
                                }

                            }
                            catch (Exception ex)
                            {
                                //return usuario;
                            }
                            finally
                            {
                                con1.Close();
                                con1.Dispose();
                                cmd1.Dispose();
                            }

                        }
                    }

                    string cadena = "";
                    if (tota1 > 0)
                    {
                        validador1 = 1;
                        cadena = cadena + tota1 + ";";
                    }
                    else
                    {
                        validador1 = 0;
                        cadena = cadena + tota1 + ";";
                    }

                    if (tota2 > 0)
                    {
                        validador2 = 1;
                        cadena = cadena + tota2 + ";";
                    }
                    else
                    {
                        validador2 = 0;
                        cadena = cadena + tota2 + ";";
                    }

                    if (tota3 > 0)
                    {
                        validador3 = 0;
                        cadena = cadena + tota3 + ";";
                    }
                    else
                    {
                        validador3 = 1;
                        cadena = cadena + tota3 + ";";
                    }

                    if ((validador1 == 1) && (validador2 == 1) && (validador3 == 1))
                    {
                        return cadena;
                    }
                    else
                    {
                        return cadena;
                    }

                }
            }
        }

        public string MuestraVersion()
        {
            string encerarBase6 = "";

            encerarBase6 = @"select ver_configuracion from configuracion where est_configuracion=1";
            string version = "";

            using (OracleConnection con1 = new OracleConnection(_conn))
            {
                using (OracleCommand cmd1 = new OracleCommand())
                {
                    OracleTransaction transaction1;
                    con1.Open();
                    cmd1.Connection = con1;
                    transaction1 = con1.BeginTransaction(IsolationLevel.ReadCommitted);
                    cmd1.Transaction = transaction1;

                    try
                    {
                        //con1.Open();
                        //cmd1.Connection = con1;
                        //cmd.CommandType = CommandType.StoredProcedure;
                        cmd1.CommandType = CommandType.Text;
                        //cmd.CommandText = "PKG_CONTEO_RAPIDO.CONSULTA_USUARIO";
                        cmd1.CommandText = string.Format(encerarBase6);
                        cmd1.BindByName = true;

                        OracleDataReader odr = cmd1.ExecuteReader();
                        if (odr.HasRows)
                        {
                            while (odr.Read())
                            {
                                version = odr["VER_CONFIGURACION"].ToString();
                            }

                            //validador1 = 1;
                        }
                        else
                        {
                            //validador1 = 0;
                        }


                    }
                    catch (Exception ex)
                    {
                        //return usuario;
                    }
                    finally
                    {
                        con1.Close();
                        con1.Dispose();
                        cmd1.Dispose();
                    }

                }
            }

            return version;
        }

        public string GeneraPDF()
        {
            String encerarBase1 = "";
            String encerarBase3 = "";
            String encerarBase4 = "";
            String encerarBase5 = "";
            String encerarBase6 = "";
            encerarBase1 = @"select count(*) TOTAL1  from acta where vot_junta=0 and bla_junta=0 and nul_junta=0 and est_acta = 0 and cod_usuario_digitador = 0 and est_descarga=0 and fec_transmision is null and cod_verresultados is null and fec_descarga is null";
            encerarBase3 = @"select count(*) TOTAL2 from resultados where fin_resultado=0 and cod_vervotos is null";
            encerarBase4 = @"select count(*) TOTAL3 from asistencia";
            encerarBase5 = @"select ver_configuracion from configuracion where est_configuracion=1";
            encerarBase6 = @"select det_configuracion from configuracion where est_configuracion=1";
            string Dato1 = "0";
            string Dato2 = "0";
            string Dato3 = "0";
            string Dato4 = "";
            string Dato5 = "";

            using (OracleConnection con1 = new OracleConnection(_conn))
            {
                using (OracleCommand cmd1 = new OracleCommand())
                {
                    OracleTransaction transaction1;
                    con1.Open();
                    cmd1.Connection = con1;
                    transaction1 = con1.BeginTransaction(IsolationLevel.ReadCommitted);
                    cmd1.Transaction = transaction1;

                    try
                    {
                        //con1.Open();
                        //cmd1.Connection = con1;
                        //cmd.CommandType = CommandType.StoredProcedure;
                        cmd1.CommandType = CommandType.Text;
                        //cmd.CommandText = "PKG_CONTEO_RAPIDO.CONSULTA_USUARIO";
                        cmd1.CommandText = string.Format(encerarBase1);
                        cmd1.BindByName = true;

                        OracleDataReader odr = cmd1.ExecuteReader();
                        if (odr.HasRows)
                        {
                            while (odr.Read())
                            {
                                Dato1 = odr["TOTAL1"].ToString();
                            }

                            //validador1 = 1;
                        }
                        else
                        {
                            //validador1 = 0;
                        }

                        cmd1.CommandText = string.Format(encerarBase3);
                        cmd1.BindByName = true;

                        OracleDataReader odr1 = cmd1.ExecuteReader();
                        if (odr1.HasRows)
                        {
                            while (odr1.Read())
                            {
                                Dato2 = odr1["TOTAL2"].ToString();
                            }

                            //validador2 = 1;
                        }
                        else
                        {
                            //validador2 = 0;
                        }

                        cmd1.CommandText = string.Format(encerarBase4);
                        cmd1.BindByName = true;

                        OracleDataReader odr2 = cmd1.ExecuteReader();
                        if (odr2.HasRows)
                        {
                            while (odr2.Read())
                            {
                                Dato3 = odr2["TOTAL3"].ToString();
                            }
                            //validador3 = 0;
                        }
                        else
                        {
                            //validador3 = 1;
                        }

                        cmd1.CommandText = string.Format(encerarBase5);
                        cmd1.BindByName = true;

                        OracleDataReader odr3 = cmd1.ExecuteReader();
                        if (odr3.HasRows)
                        {
                            while (odr3.Read())
                            {
                                Dato4 = odr3["VER_CONFIGURACION"].ToString();
                            }
                            //validador3 = 0;
                        }
                        else
                        {
                            //validador3 = 1;
                        }

                        cmd1.CommandText = string.Format(encerarBase6);
                        cmd1.BindByName = true;

                        OracleDataReader odr4 = cmd1.ExecuteReader();
                        if (odr4.HasRows)
                        {
                            while (odr4.Read())
                            {
                                Dato5 = odr4["DET_CONFIGURACION"].ToString();
                            }
                            //validador3 = 0;
                        }
                        else
                        {
                            //validador3 = 1;
                        }

                    }
                    catch (Exception ex)
                    {
                        //return usuario;
                    }
                    finally
                    {
                        con1.Close();
                        con1.Dispose();
                        cmd1.Dispose();
                    }

                }
            }


            Document doc = new Document(PageSize.LETTER);
            // Indicamos donde vamos a guardar el documento
            //PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream("C:\\Reporte.pdf", FileMode.Create));

            var pdfStream = new FileStream("Reporte.pdf", FileMode.Create);

            PdfWriter writer = PdfWriter.GetInstance(doc, pdfStream);

            // Le colocamos el título y el autor
            // **Nota: Esto no será visible en el documento
            doc.AddTitle("Reporte Enceramiento Datos");
            doc.AddCreator("Consejo Nacional Electoral.");

            // Abrimos el archivo
            doc.Open();

            // Creamos el tipo de Font que vamos utilizar
            iTextSharp.text.Font _standardFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);

            // Escribimos el encabezamiento en el documento
            doc.Add(new Paragraph("Reporte Enceramiento Datos"));
            doc.Add(Chunk.NEWLINE);

            doc.Add(new Paragraph("Proceso:"));
            doc.Add(Chunk.NEWLINE);
            doc.Add(new Paragraph(Dato5));
            doc.Add(Chunk.NEWLINE);

            doc.Add(new Paragraph("Version:"));
            doc.Add(Chunk.NEWLINE);
            doc.Add(new Paragraph(Dato4));
            doc.Add(Chunk.NEWLINE);


            // Creamos una tabla que 
            PdfPTable tblReporte1 = new PdfPTable(2);
            tblReporte1.WidthPercentage = 100;

            // Configuramos el título de las columnas de la tabla
            PdfPCell clProceso = new PdfPCell(new Phrase("Proceso: ", _standardFont));
            clProceso.BorderWidth = 0;
            clProceso.BorderWidthBottom = 0.75f;

            PdfPCell clVersion = new PdfPCell(new Phrase("Version: ", _standardFont));
            clVersion.BorderWidth = 0;
            clVersion.BorderWidthBottom = 0.75f;

            // Añadimos las celdas a la tabla
            tblReporte1.AddCell(clProceso);
            tblReporte1.AddCell(clVersion);

            // Llenamos la tabla con información
            clProceso = new PdfPCell(new Phrase(Dato4, _standardFont));
            clProceso.BorderWidth = 0;

            clVersion = new PdfPCell(new Phrase(Dato5, _standardFont));
            clVersion.BorderWidth = 0;


            // Añadimos las celdas a la tabla
            tblReporte1.AddCell(clProceso);
            tblReporte1.AddCell(clVersion);

            doc.Add(tblReporte1);

            doc.Add(new Paragraph(" "));
            doc.Add(Chunk.NEWLINE);

            // Creamos una tabla que 
            PdfPTable tblReporte = new PdfPTable(3);
            tblReporte.WidthPercentage = 100;

            // Configuramos el título de las columnas de la tabla
            PdfPCell clActas = new PdfPCell(new Phrase("Registro Actas Enceradas", _standardFont));
            clActas.BorderWidth = 0;
            clActas.BorderWidthBottom = 0.75f;

            PdfPCell clResultados = new PdfPCell(new Phrase("Registro Resultados Encerados", _standardFont));
            clResultados.BorderWidth = 0;
            clResultados.BorderWidthBottom = 0.75f;

            PdfPCell clAsistencia = new PdfPCell(new Phrase("Registro Asistencia sin Encerar", _standardFont));
            clAsistencia.BorderWidth = 0;
            clAsistencia.BorderWidthBottom = 0.75f;

            // Añadimos las celdas a la tabla
            tblReporte.AddCell(clActas);
            tblReporte.AddCell(clResultados);
            tblReporte.AddCell(clAsistencia);

            // Llenamos la tabla con información
            clActas = new PdfPCell(new Phrase(Dato1, _standardFont));
            clActas.BorderWidth = 1;

            clResultados = new PdfPCell(new Phrase(Dato2, _standardFont));
            clResultados.BorderWidth = 1;

            clAsistencia = new PdfPCell(new Phrase(Dato3, _standardFont));
            clAsistencia.BorderWidth = 1;

            // Añadimos las celdas a la tabla
            tblReporte.AddCell(clActas);
            tblReporte.AddCell(clResultados);
            tblReporte.AddCell(clAsistencia);

            doc.Add(tblReporte);

            doc.Add(new Paragraph("Consejo Nacional Electoral."));
            doc.Add(Chunk.NEWLINE);

            doc.Close();
            writer.Close();



            string version = "";
            return version;
        }

    }
}
