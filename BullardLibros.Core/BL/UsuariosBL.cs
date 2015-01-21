using BullardLibros.Core.DTO;
using BullardLibros.Data;
using BullardLibros.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
//using System.Data.Objects.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//required for sql function access
using System.Data.Entity.Core.Objects.DataClasses;

namespace BullardLibros.Core.BL
{
    public class UsuariosBL : Base
    {
        public IList<UsuarioDTO> getUsuarios()
        {
            using (var context = getContext())
            {
                var result = from r in context.Usuario
                             where r.Estado == true & r.IdRol != CONSTANTES.SUPER_ADMIN_ROL
                             select new UsuarioDTO
                             {
                                 IdUsuario = r.IdUsuario,
                                 Nombre = r.Nombre,
                                 Email = r.Email,
                                 Cuenta = r.Cuenta,
                                 Pass = r.Pass,
                                 Active = r.Estado,
                                 IdRol = r.IdRol //?? 0
                             };
                return result.AsEnumerable<UsuarioDTO>().OrderByDescending(x => x.IdUsuario).ToList<UsuarioDTO>();
            }
        }
        public IList<UsuarioDTO> getUsuarios(int IdRol)
        {
            using (var context = getContext())
            {
                var result = from r in context.Usuario.AsEnumerable()
                             where getRoleKeys(IdRol).Contains(r.IdRol)//& r.IdRol != CONSTANTES.SUPER_ADMIN_ROL & r.Estado == true
                             select new UsuarioDTO
                             {
                                 IdUsuario = r.IdUsuario,
                                 Nombre = r.Nombre,
                                 Email = r.Email,
                                 Cuenta = r.Cuenta,
                                 Active = r.Estado,
                                 IdRol = r.IdRol //?? 0
                             };
                return result.ToList<UsuarioDTO>();//.AsEnumerable<UsuarioDTO>().OrderByDescending(x => x.Nombre).ToList<UsuarioDTO>();
            }
        }

        public int[] getRoleKeys(int IdRol)
        {
            var roles = new int[1];
            if (IdRol == CONSTANTES.SUPER_ADMIN_ROL) roles = new int[] { CONSTANTES.ROL_ADMIN, CONSTANTES.ROL_RESPONSABLE };
            if (IdRol == CONSTANTES.ROL_ADMIN) roles = new int[] { CONSTANTES.ROL_RESPONSABLE };
            if (IdRol == CONSTANTES.ROL_RESPONSABLE) roles = new int[] { CONSTANTES.ROL_RESPONSABLE };
            return roles;
        }

        public IList<UsuarioDTO> getUsuarios(int IdRol, int[] usuariosIds)
        {
            using (var context = getContext())
            {
                var result = from r in context.Usuario
                             where r.Estado == true & r.IdRol != CONSTANTES.SUPER_ADMIN_ROL & r.IdRol == IdRol & usuariosIds.Contains(r.IdUsuario)
                             select new UsuarioDTO
                             {
                                 IdUsuario = r.IdUsuario,
                                 Nombre = r.Nombre,
                                 Email = r.Email,
                                 Cuenta = r.Cuenta,
                                 Active = r.Estado,
                                 IdRol = r.IdRol //?? 0
                             };
                return result.AsEnumerable<UsuarioDTO>().OrderByDescending(x => x.Nombre).ToList<UsuarioDTO>();
            }
        }

        public System.Collections.IList getUsuarios2(int IdRol)
        {
            using (var context = getContext())
            {
                var result = from r in context.Usuario
                             where r.Estado == true & r.IdRol != CONSTANTES.SUPER_ADMIN_ROL & r.IdRol == IdRol
                             select new
                             {
                                 name = r.Nombre,
                                 id = r.IdUsuario,
                                 //tareas = context.Tarea.Where(x => x.IdResponsable == r.IdUsuario).Select(y => new { IdTarea = y.IdTarea, Nombre = y.Nombre, FechaInicio = y.FechaInicio, FechaFin = y.FechaFin })
                             };
                return result.ToList();//.AsEnumerable().OrderByDescending(x => x.name).ToList();
            }
        }

        public bool add(UsuarioDTO user)
        {
            using (var context = getContext())
            {
                try
                {
                    Usuario usuario = new Usuario();
                    usuario.Nombre = user.Nombre;
                    usuario.InicialesNombre = user.InicialesNombre;
                    usuario.Email = user.Email;
                    usuario.Cuenta = user.Cuenta;
                    usuario.Pass = Encrypt.GetCrypt(user.Pass);
                    usuario.IdRol = user.IdRol;//>= 2 ? user.IdRol : 3;
                    //usuario.IdCargo = user.IdCargo;
                    usuario.Estado = true;
                    usuario.FechaRegistro = DateTime.Now;
                    context.Usuario.Add(usuario);
                    context.SaveChanges();
                    return true;
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
        public bool validateUsuario(UsuarioDTO user)
        {
            using (var context = getContext())
            {
                if (user.Cuenta != null && user.Email != null)
                {
                    var result = from r in context.Usuario
                                 where r.Cuenta == user.Cuenta || r.Email == user.Email
                                 select r;
                    if (result.FirstOrDefault<Usuario>() == null)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public List<RolDTO> getRoles()
        {
            using (var context = getContext())
            {
                var result = context.Rol.Where(x => x.IdRol != CONSTANTES.SUPER_ADMIN_ROL).Select(r => new RolDTO
                {
                    IdRol = r.IdRol,
                    Nombre = r.Nombre
                }).ToList();

                return result;
            }
        }

        public List<SelectDTO> getSelectRoles()
        {
            using (var context = getContext())
            {
                var result = context.Rol.Where(x => x.IdRol != CONSTANTES.SUPER_ADMIN_ROL).Select(r => new SelectDTO
                    {
                        SelectItemId = r.IdRol,
                        SelectItemName = r.Nombre
                    }).ToList();
                return result;
            }
        }

        public IList<SelectDTO> getRolesViewBag(bool AsSelectList = false)
        {
            if (!AsSelectList)
            {
                return getSelectRoles();
            }
            else
            {
                var lista = getSelectRoles();
                lista.Insert(0, new SelectDTO() { SelectItemId = 0, SelectItemName = "Seleccione el Tipo de Usuario." });
                return lista;
            }
        }

        public UsuarioDTO getUsuarioByCuenta(UsuarioDTO user)
        {
            using (var context = getContext())
            {
                var result = from r in context.Usuario
                             where r.Estado == true & r.Cuenta == user.Cuenta
                             select new UsuarioDTO
                             {
                                 IdUsuario = r.IdUsuario,
                                 Nombre = r.Nombre,
                                 IdRol = r.IdRol,// ?? 0,
                                 Active = r.Estado,
                                 Email = r.Email,
                                 Pass = r.Pass,
                                 Cuenta = r.Cuenta
                             };
                return result.SingleOrDefault<UsuarioDTO>();
            }
        }

        public bool isValidUser(UsuarioDTO user)
        {
            using (var context = getContext())
            {
                var result = from r in context.Usuario
                             where r.Estado == true & r.Cuenta == user.Cuenta
                             select r;
                Usuario usuario = result.SingleOrDefault<Usuario>();
                if (usuario != null)
                {
                    if (Encrypt.comparetoCrypt(user.Pass, usuario.Pass))
                        return true;
                }
            }
            return false;
        }

        public UsuarioDTO getUsuario(int id)
        {
            using (var context = getContext())
            {
                var result = context.Usuario.Where(x => x.IdUsuario == id).Select(r => new UsuarioDTO
                    {
                        IdUsuario = r.IdUsuario,
                        Nombre = r.Nombre,
                        InicialesNombre = r.InicialesNombre,
                        Email = r.Email,
                        Cuenta = r.Cuenta,
                        Pass = r.Pass,
                        Active = r.Estado,
                        IdRol = r.IdRol,
                        IdCargo = r.IdCargo
                    }).SingleOrDefault();
                return result;
            }
        }

        public bool update(UsuarioDTO user, string passUser, string passChange, UsuarioDTO currentUser)
        {
            using (var context = getContext())
            {
                try
                {
                    Usuario usuario = context.Usuario.Where(x => x.IdUsuario == user.IdUsuario).SingleOrDefault();
                    if (usuario != null)
                    {
                        usuario.Nombre = user.Nombre;
                        usuario.InicialesNombre = user.InicialesNombre;
                        usuario.Email = user.Email;
                        usuario.IdRol = user.IdRol;// >= 2 ? user.IdRol : 3;
                        //usuario.IdCargo = user.IdCargo;
                        usuario.Cuenta = user.Cuenta;
                        usuario.Estado = user.Active;
                        if (!String.IsNullOrWhiteSpace(passUser) && !String.IsNullOrWhiteSpace(passChange))
                        {
                            if ((currentUser.IdRol <= 2 || currentUser.IdUsuario == user.IdUsuario)
                                && Encrypt.comparetoCrypt(passUser, currentUser.Pass))
                            {
                                usuario.Pass = Encrypt.GetCrypt(passChange);
                            }
                            else return false;
                        }
                        context.SaveChanges();
                        return true;
                    }
                }
                catch (Exception e)
                {
                    //throw e;
                    return false;
                }
                return false;
            }
        }

        public bool recoverPassword(string CuentaEmail)
        {
            using (var context = getContext())
            {
                var result = (from r in context.Usuario
                              where r.Cuenta.Equals(CuentaEmail) || r.Email.Equals(CuentaEmail) && r.Estado == true
                              select new UsuarioDTO
                              {
                                  IdUsuario = r.IdUsuario,
                                  Nombre = r.Nombre,
                                  Email = r.Email
                              }).FirstOrDefault();
                if (result != null)
                {
                    string newPassword = Functions.GeneratePassword();
                    updatePassword(result, newPassword);
                    SendMailPassRecovery(result, newPassword);
                    return true;
                }
                else
                    return false;
            }
        }
        public bool updatePassword(UsuarioDTO user, string passChange)
        {
            using (var context = getContext())
            {
                try
                {
                    Usuario usuario = context.Usuario.Where(x => x.IdUsuario == user.IdUsuario).SingleOrDefault();
                    usuario.Pass = Encrypt.GetCrypt(passChange);
                    context.SaveChanges();
                    return true;
                }
                catch (Exception e)
                {
                    //throw e;
                    return false;
                }
            }
        }
        public void SendMailPassRecovery(UsuarioDTO user, string passChange)
        {
            string to = user.Email;
            string subject = "Recuperación de Contraseña";
            string body = "Sr(a). " + user.Nombre + " su contraseña es : " + passChange;
            MailHandler.Send(to, "", subject, body);
        }
        #region
        public IList<UsuarioDTO> searchResponsables(string busqueda)
        {
            using (var context = getContext())
            {
                return (from r in context.Usuario
                        where r.IdRol != CONSTANTES.SUPER_ADMIN_ROL
                        & r.Nombre.Contains(busqueda)
                        & r.Estado == true
                        select new UsuarioDTO
                        {
                            IdUsuario = r.IdUsuario,
                            Nombre = r.Nombre,
                            InicialesNombre = r.InicialesNombre,
                            Email = r.Email
                        }).ToList();
            }
        }
        #endregion
    }
}
