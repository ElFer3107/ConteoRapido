using CoreCRUDwithORACLE.Interfaces;
using CoreCRUDwithORACLE.Models;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using CoreCRUDwithORACLE.Comunes;

namespace CoreCRUDwithORACLE.Servicios
{
    public class ServicioActa : IServicioActa
    {
        private readonly string _conn;
        private Comunes.Auxiliar _helper = new Comunes.Auxiliar();
        private readonly IDataProtector protector;
        

        public ServicioActa(IConfiguration _configuration, IDataProtectionProvider dataProtectionProvider, Helper dataHelper)
        {
            _conn = _configuration.GetConnectionString("OracleDBConnection");
            this.protector = dataProtectionProvider.CreateProtector(dataHelper.CodigoEnrutar);
        }

        private string consultaActas = @" SELECT P.COD_PROVINCIA, P.NOM_PROVINCIA, C.COD_CANTON,C.NOM_CANTON,Q.COD_PARROQUIA, 
                                                 Q.NOM_PARROQUIA, Z.COD_ZONA,Z.NOM_ZONA, J.JUNTA, J.SEXO, a.est_Acta as Estado_Acta,
                                                 A.COD_JUNTA, A.COD_USUARIO, U.NOM_USUARIO, VOT_JUNTA,BLA_JUNTA,NUL_JUNTA       
                                                 FROM PROVINCIA P, CANTON C, PARROQUIA Q , ZONA Z, JUNTA J, ACTA A, USUARIO U
                                                 WHERE J.COD_ZONA=Z.COD_ZONA
                                                 AND Z.COD_PARROQUIA=Q.COD_PARROQUIA
                                                 AND J.COD_PARROQUIA=Q.COD_PARROQUIA
                                                 AND J.COD_CANTON=C.COD_CANTON
                                                 AND J.COD_PROVINCIA=P.COD_PROVINCIA
                                                 AND A.COD_JUNTA=J.COD_JUNTA
                                                 AND A.COD_USUARIO = U.COD_USUARIO(+)";

        private string consultaAsignacion = @" select cod_junta
                                         from acta 
                                         where cod_usuario = {0}";

        private string consultaResultadosCand = @"SELECT R.COD_JUNTA, R.COD_CANDIDATO, C.NOM_CANDIDATO, R.FIN_RESULTADO, c.ORD_CANDIDATO
                                                    FROM RESULTADOS R, ACTA A, CANDIDATO C
                                                    WHERE R.COD_JUNTA = A.COD_JUNTA
                                                    AND R.COD_CANDIDATO = C.COD_CANDIDATO
                                                    AND A.COD_JUNTA = {0} 
                                                    ORDER BY 5";

        private string consultaResultadosActa = @"SELECT P.COD_PROVINCIA, P.NOM_PROVINCIA, C.COD_CANTON,C.NOM_CANTON,Q.COD_PARROQUIA, 
                                                    Q.NOM_PARROQUIA, Z.COD_ZONA,Z.NOM_ZONA, J.JUNTA || J.SEXO AS JUNTA,
                                                    A.COD_JUNTA, A.COD_USUARIO,  VOT_JUNTA,BLA_JUNTA,NUL_JUNTA, J.NUMELE_JUNTA, A.EST_ACTA    
                                                    FROM PROVINCIA P, CANTON C, PARROQUIA Q , ZONA Z, JUNTA J, ACTA A
                                                    WHERE J.COD_ZONA=Z.COD_ZONA
                                                    AND Z.COD_PARROQUIA=Q.COD_PARROQUIA
                                                    AND J.COD_PARROQUIA=Q.COD_PARROQUIA
                                                    AND J.COD_CANTON=C.COD_CANTON
                                                    AND J.COD_PROVINCIA=P.COD_PROVINCIA
                                                    AND A.COD_JUNTA=J.COD_JUNTA
                                                    AND A.COD_JUNTA = {0}";

        private string actualizaActa = @" update acta 
                                          set cod_usuario = {0}
                                         where cod_junta = {1}";

        private string actualizarVotosActa = @"UPDATE CONTEORAPIDO2021.ACTA
                                        SET    VOT_JUNTA             = {0},
                                               BLA_JUNTA             = {1},
                                               NUL_JUNTA             = {2},
                                               FEC_JUNTA             = sysdate,
                                               NOV_ACTA              = {4},
                                               TIPO_DOCUMENTO        = {5},
                                               COD_USUARIO_DIGITADOR = {6},
                                               EST_ACTA              = {7},
                                               FEC_TRANSMISION       = sysdate,
                                               COD_VERRESULTADOS     = '{8}'
                                           WHERE COD_JUNTA             = {9} AND
                                                EST_ACTA             = 0";

        private string actualizaResultado = @"UPDATE CONTEORAPIDO2021.RESULTADOS
                                            SET    FIN_RESULTADO             = {0},
                                                   COD_VERVOTOS             = '{1}'
                                           WHERE COD_JUNTA             = {2} AND
                                                 COD_CANDIDATO          = {3}";
        public IEnumerable<ActaResponse> GetActas(int codigoProvincia)
        {
            List<ActaResponse> actas = null;

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
                        if (codigoProvincia != 0)
                            consultaActas = consultaActas + " AND J.COD_PROVINCIA = {0} ";

                        cmd.CommandText = string.Format(consultaActas, codigoProvincia);
                        cmd.BindByName = true;

                        OracleDataReader odr = cmd.ExecuteReader();

                        if (odr.HasRows)
                        {
                            actas = new List<ActaResponse>();
                            while (odr.Read())
                            {
                                ActaResponse acta = new ActaResponse
                                {
                                    USUARIO = Convert.ToString(odr["NOM_USUARIO"]),
                                    Cod_Canton = Convert.ToInt32(odr["COD_CANTON"]),
                                    Cod_Parroquia = Convert.ToInt32(odr["COD_PARROQUIA"]),
                                    Cod_Provincia = Convert.ToInt32(odr["COD_PROVINCIA"]),
                                    Cod_Zona = Convert.ToInt32(odr["COD_ZONA"]),
                                    junta = Convert.ToString(odr["JUNTA"]),
                                    Nom_Canton = Convert.ToString(odr["NOM_CANTON"]),
                                    Nom_Parroquia = Convert.ToString(odr["NOM_PARROQUIA"]),
                                    Nom_Provincia = Convert.ToString(odr["NOM_PROVINCIA"]),
                                    Nom_Zona = Convert.ToString(odr["NOM_ZONA"]),
                                    sexo = Convert.ToString(odr["SEXO"]),
                                    COD_JUNTA = Convert.ToInt32(odr["cod_junta"]),
                                    COD_USUARIO = Convert.ToInt32(odr["cod_usuario"]),
                                    BLA_JUNTA = Convert.ToInt32(odr["bla_junta"]),
                                    NUL_JUNTA = Convert.ToInt32(odr["nul_junta"]),
                                    VOT_JUNTA = Convert.ToInt32(odr["vot_junta"]),
                                    Estado_Acta= Convert.ToInt32(odr["Estado_Acta"])
                                };
                                actas.Add(acta);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        return actas;
                    }
                    finally
                    {
                        con.Close();
                        con.Dispose();
                        cmd.Dispose();
                    }

                }
            }

            return actas;
        }
        public ActaResponse GetActa(int junta)
        {
            ActaResponse acta = null;

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
                        consultaActas = consultaActas + " AND J.cod_junta = {0}";
                        cmd.CommandText = string.Format(consultaActas, junta);
                        cmd.BindByName = true;

                        OracleDataReader odr = cmd.ExecuteReader();

                        if (odr.HasRows)
                        {
                            while (odr.Read())
                            {
                                acta = new ActaResponse
                                {
                                    COD_JUNTA = Convert.ToInt32(odr["cod_junta"]),
                                    COD_USUARIO = Convert.ToInt32(odr["cod_usuario"]),
                                    BLA_JUNTA = Convert.ToInt32(odr["bla_junta"]),
                                    NUL_JUNTA = Convert.ToInt32(odr["nul_junta"]),
                                    VOT_JUNTA = Convert.ToInt32(odr["vot_junta"]),
                                    Nom_Canton = Convert.ToString(odr["NOM_CANTON"]),
                                    Nom_Parroquia = Convert.ToString(odr["NOM_PARROQUIA"]),
                                    Nom_Zona = Convert.ToString(odr["NOM_ZONA"])
                                };
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        return acta = null;
                    }
                    finally
                    {
                        con.Close();
                        con.Dispose();
                        cmd.Dispose();
                    }

                }
            }

            return acta;
        }
        public int ActualizaActa(int cod_usuario, int junta)
        {
            int resultado = 0;

            using (OracleConnection con = new OracleConnection(_conn))
            {
                using (OracleCommand cmd = new OracleCommand())
                {
                    try
                    {

                        OracleTransaction transaction;
                        con.Open();
                        cmd.Connection = con;
                        transaction = con.BeginTransaction(IsolationLevel.ReadCommitted);
                        cmd.Transaction = transaction;
                        transaction.Commit();

                                                //con.Open();
                        //cmd.Connection = con;
                        //cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandType = CommandType.Text;
                        //cmd.CommandText = "PKG_CONTEO_RAPIDO.CONSULTA_USUARIO";
                        cmd.CommandText = string.Format(actualizaActa, cod_usuario, junta);
                        
                        return resultado = cmd.ExecuteNonQuery();

                    }
                    catch (Exception ex)
                    {
                        return resultado;
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

        public Acta ConsultarAsignacion(int codigoUsuario)
        {
            Acta acta = null;

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
                        cmd.CommandText = string.Format(consultaAsignacion, codigoUsuario);

                        OracleDataReader odr = cmd.ExecuteReader();

                        if (odr.HasRows)
                        {
                            while (odr.Read())
                            {
                                acta = new Acta
                                {
                                    COD_JUNTA = Convert.ToInt32(odr["cod_junta"])
                                };
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        return acta = null;
                    }
                    finally
                    {
                        con.Close();
                        con.Dispose();
                        cmd.Dispose();
                    }

                }
            }

            return acta;
        }

        public int ActualizaAsignacion(int codigoJunta)
        {
            int resultado = 0;

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
                        cmd.CommandText = string.Format(actualizaActa, 0, codigoJunta);

                        return resultado = cmd.ExecuteNonQuery();

                    }
                    catch (Exception ex)
                    {
                        return resultado;
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

        public ResultadosVotos ConsultaResultados(string? codigoJunta)
        {
            ResultadosVotos resultados = null;
            List<Resultado> resultadosVotos = null;
            Resultado resultado = null;
            Acta acta = null;
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
                        //cmd.CommandText = "PKG_CONTEO_RAPIDO.CONSULTA_USUARIO";
                        var cod_junta = protector.Unprotect(codigoJunta);
                        int codJunta = Convert.ToInt32(cod_junta);
                        cmd.CommandText = string.Format(consultaResultadosActa, codJunta);

                        OracleDataReader odr = cmd.ExecuteReader();

                        if (odr.HasRows)
                        {
                            while (odr.Read())
                            {
                                acta = new Acta
                                {
                                    COD_JUNTA = Convert.ToInt32(odr["COD_JUNTA"]),
                                    VOT_JUNTA = Convert.ToInt32(odr["VOT_JUNTA"]),
                                    BLA_JUNTA = Convert.ToInt32(odr["BLA_JUNTA"]),
                                    NUL_JUNTA = Convert.ToInt32(odr["NUL_JUNTA"]),
                                    PROVINCIA = Convert.ToString(odr["NOM_PROVINCIA"]),
                                    CANTON = Convert.ToString(odr["NOM_CANTON"]),
                                    PARROQUIA = Convert.ToString(odr["NOM_PARROQUIA"]),
                                    ZONA = Convert.ToString(odr["NOM_ZONA"]),
                                    JUNTA = Convert.ToString(odr["JUNTA"]),
                                    TOT_ELECTORES = Convert.ToInt32(odr["NUMELE_JUNTA"]),
                                    Estado_Acta = Convert.ToInt32(odr["EST_ACTA"])
                                };
                            }
                        }
                        else
                        {
                            return resultados = null;
                        }

                        cmd.CommandType = CommandType.Text;
                        //cmd.CommandText = "PKG_CONTEO_RAPIDO.CONSULTA_USUARIO";
                        cmd.CommandText = string.Format(consultaResultadosCand, codJunta);

                        odr = cmd.ExecuteReader();

                        if (odr.HasRows)
                        {
                            resultadosVotos = new List<Resultado>();
                            while (odr.Read())
                            {
                                resultado = new Resultado
                                {
                                    Candidato = Convert.ToString(odr["NOM_CANDIDATO"]),
                                    Cod_Candidato = Convert.ToInt32(odr["COD_CANDIDATO"]),
                                    Orden = Convert.ToInt32(odr["ORD_CANDIDATO"]),
                                    VOTOS = Convert.ToInt32(odr["FIN_RESULTADO"])
                                };
                                resultadosVotos.Add(resultado);
                            }
                        }
                        else
                            return resultados = null;

                        transaction.Commit();
                        resultados = new ResultadosVotos()
                        {
                            Acta = acta,
                            Resultados = resultadosVotos
                        };

                        return resultados;

                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return resultados = null;
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

        public Respuesta ActualizarVotosActa(ResultadosVotos resultados)
        {
            Acta acta = new Acta();
            acta = resultados.Acta;
            string codigoVerificacion = string.Empty;
            string codigoVerificacionv = string.Empty;
            Respuesta respuesta = new Respuesta();

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
                        
                        cmd.CommandType = CommandType.Text;
                        codigoVerificacion = _helper.EncodePassword(acta.COD_USUARIO + acta.JUNTA + DateTime.Now.ToString() + acta.VOT_JUNTA);
                        cmd.CommandText = string.Format(actualizarVotosActa, acta.VOT_JUNTA, acta.BLA_JUNTA, acta.NUL_JUNTA,
                                                        DateTime.Now.ToString(), acta.NOV_ACTA, 1, acta.COD_USUARIO, 1, codigoVerificacion,
                                                        acta.COD_JUNTA, acta.COD_USUARIO);
                        
                        int resultadoActa = cmd.ExecuteNonQuery();
                        if (resultadoActa > 0)
                        {
                            foreach (var voto in resultados.Resultados)
                            {
                                //codigoVerificacionv = _helper.EncodePassword( DateTime.Now.ToString() + voto.Candidato);
                                //cmd.CommandText = string.Format(actualizaResultado, codigoVerificacionv, acta.COD_JUNTA);
                                codigoVerificacionv = _helper.EncodePassword(voto.VOTOS + voto.Cod_Candidato + DateTime.Now.ToString() + voto.Candidato);
                                cmd.CommandText = string.Format(actualizaResultado, voto.VOTOS, codigoVerificacionv, acta.COD_JUNTA, voto.Cod_Candidato);
                                int resultadoVoto = cmd.ExecuteNonQuery();
                                if (resultadoVoto == 0)
                                {
                                    transaction.Rollback();
                                    respuesta.codigoResultado = 3;
                                    return respuesta;
                                }
                            }
                            transaction.Commit();
                            respuesta.CodigoVerificacion = codigoVerificacion;
                            respuesta.codigoResultado = 1;
                        }
                        else
                        {
                            transaction.Rollback();
                            respuesta.codigoResultado = 4;
                        }
                    }
                    catch (Exception EX)
                    {
                        transaction.Rollback();
                        respuesta.codigoResultado = 2;
                        respuesta.Mensaje = EX.Message;
                        return respuesta;
                    }
                    finally
                    {
                        con.Dispose();
                        con.Close();
                    }
                }
            }
            return respuesta;
        }
    }
}
