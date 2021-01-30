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

        private string consultaLogin = @"select count(*)
                                        from USUARIO
                                        where MAI_USUARIO = '{0}'
                                        and CLA_USUARIO = '{1}'";

        private string consultaUsuario = @"select COD_ROL, NOM_USUARIO, COD_PROVINCIA, EST_CLA_USUARIO, CED_USUARIO || DIG_USUARIO CEDULA
                                        from USUARIO
                                        where MAI_USUARIO = '{0}'
                                        and CLA_USUARIO = '{1}'
                                        and EST_USUARIO = 1";

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
                                    NOMBRE = Convert.ToString(odr["NOM_USUARIO"])
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
                if (usuario.CLAVE == clave)
                    return usuario = null;

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


                            cmd.CommandText = string.Format(actualizaUsuarioClave, clave, estado,
                                                            usuarioNew.CEDULA.Substring(0, 9));

                            int odr = cmd.ExecuteNonQuery();

                            if (odr == 0)
                                usuario = null;
                        }
                        catch (Exception ex)
                        {
                            return usuario = null;
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

        public int EncerarBase()
        {
            string encerarBase1 = @"update acta set vot_junta=0, bla_junta=0, nul_junta=0, est_acta = 0, cod_usuario_digitador = 0, fec_transmision = null, cod_verresultados='', fec_descarga=null,est_descarga=0";
            string encerarBase3 = @"delete from asistencia";
            string encerarBase4 = @"update  resultados set  fin_resultado=0, cod_vervotos = ''";
            string encerarBase5 = @"update configuracion set est_configuracion = 0";
            string encerarBase6 = @"insert into configuracion values (10,'Prueba Interna','1.0','28/01/2021',1)";

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
                        return 1;

                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return 0;
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
    }
}
