using BullardLibros.Core.BL;
using BullardLibros.Core.DTO;
using BullardLibros.Helpers;
using BullardLibros.Helpers.Razor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.UI.WebControls;
using System.Web.UI;
using PagedList;
using PagedList.Mvc;
using System.Globalization;

namespace BullardLibros.Controllers
{
    public class AdminController : Controller
    {
        private bool currentUser()
        {
            if (System.Web.HttpContext.Current.Session != null && System.Web.HttpContext.Current.Session["User"] != null) { return true; }
            else { return false; }
        }
        private UsuarioDTO getCurrentUser()
        {
            if (this.currentUser())
            {
                return (UsuarioDTO)System.Web.HttpContext.Current.Session["User"];
            }
            return null;
        }
        private bool isSuperAdministrator()
        {
            if (getCurrentUser().IdRol == 1) return true;
            return false;
        }
        private bool isAdministrator()
        {
            if (getCurrentUser().IdRol <= 2) return true;
            return false;
        }
        private void createResponseMessage(string status, string message = "", string status_field = "status", string message_field = "message")
        {
            TempData[status_field] = status;
            if (!String.IsNullOrWhiteSpace(message))
            {
                TempData[message_field] = message;
            }
        }

        public AdminController()
        {
            UsuarioDTO user = getCurrentUser();
            if (user != null)
            {
                ViewBag.currentUser = user;
                ViewBag.EsAdmin = isAdministrator();
                ViewBag.EsSuperAdmin = isSuperAdministrator();
                ViewBag.IdRol = user.IdRol;
            }
            else { ViewBag.EsAdmin = false; ViewBag.EsSuperAdmin = false; }
        }

        public ActionResult Ingresar()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(UsuarioDTO user)
        {
            if (ModelState.IsValid)
            {
                UsuariosBL usuariosBL = new UsuariosBL();
                if (usuariosBL.isValidUser(user))
                {
                    System.Web.HttpContext.Current.Session["User"] = usuariosBL.getUsuarioByCuenta(user);//new UsuarioDTO() { Nombre = "NubeLabs", IdUsuario = 1, IdRol = 1 }; //{ Nombre = "Responsable 1", IdUsuario = 2, IdRol = 3 };  //usuariosBL.getUsuarioByCuenta(user);
                    return RedirectToAction("Index");
                }
            }
            return RedirectToAction("Ingresar");
        }
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Ingresar");
        }

        public ActionResult Formulario()
        { return View(); }

        public ActionResult Index()
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            CuentaBancariaBL objBL = new CuentaBancariaBL();
            return View(objBL.getCuentasBancarias());
        }

        public ActionResult Libro(int? id = null, int? page = null)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            //if (!this.isAdministrator()) { return RedirectToAction("Index"); }
            CuentaBancariaBL objBL = new CuentaBancariaBL();
            ViewBag.IdCuentaBancaria = id;
            var objSent = TempData["Libro"];
            if (objSent != null) { TempData["Libro"] = null; return View(objSent); }
            if (id != null)
            {
                CuentaBancariaDTO obj = objBL.getCuentaBancaria((int)id);
                int pageSize = 20;
                int pageNumber = (page ?? 1);
                obj.listaMovimientoPL = obj.listaMovimiento.ToPagedList(pageNumber, pageSize);
                return View(obj);
            }
            return View();
        }

        [HttpPost]
        public ActionResult AddLibro(CuentaBancariaDTO dto)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            try
            {
                CuentaBancariaBL objBL = new CuentaBancariaBL();
                if (dto.IdCuentaBancaria == 0)
                {
                    if (objBL.add(dto))
                    {
                        createResponseMessage(CONSTANTES.SUCCESS);
                        return RedirectToAction("Index");
                    }
                }
                else if (dto.IdCuentaBancaria != 0)
                {
                    if (objBL.update(dto))
                    {
                        createResponseMessage(CONSTANTES.SUCCESS);
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_UPDATE_MESSAGE);
                    }

                }
                else
                {
                    createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_INSERT_MESSAGE);
                }
            }
            catch (Exception e)
            {
                if (dto.IdCuentaBancaria != 0)
                    createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_UPDATE_MESSAGE);
                else createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_INSERT_MESSAGE);
            }
            TempData["Libro"] = dto;
            return RedirectToAction("Libro");
        }

        public ActionResult Categorias()
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            CategoriaBL objBL = new CategoriaBL();
            return View(objBL.getCategoriasTree());
        }

        public ActionResult Categoria(int? id = null, int? idPadre = null)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            //if (!this.isAdministrator()) { return RedirectToAction("Index"); }
            CategoriaBL objBL = new CategoriaBL();
            ViewBag.IdCategoria = id;
            ViewBag.Categorias = objBL.getCategoriasPadre(true);
            ViewBag.NombreCategoria = "Sin Categoría";
            var objSent = TempData["Categoria"];
            if (objSent != null) { TempData["Categoria"] = null; return View(objSent); }
            if (id != null || id == 0)
            {
                if (idPadre != null)
                {
                    CategoriaDTO objp = new CategoriaDTO();
                    objp.IdCategoria = 0;
                    objp.IdCategoriaPadre = idPadre;
                    objp.Orden = objBL.getUltimoHijo(idPadre.GetValueOrDefault());
                    ViewBag.NombreCategoria = objBL.getNombreCategoria(objp.IdCategoriaPadre.GetValueOrDefault());
                    return View(objp);
                }
                CategoriaDTO obj = objBL.getCategoria((int)id);
                ViewBag.NombreCategoria = objBL.getNombreCategoria(obj.IdCategoriaPadre.GetValueOrDefault());
                return View(obj);
            }
            return View();
        }

        [HttpPost]
        public ActionResult AddCategoria(CategoriaDTO dto)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            try
            {
                CategoriaBL objBL = new CategoriaBL();
                if (dto.IdCategoria == 0)
                {
                    if (objBL.add(dto))
                    {
                        createResponseMessage(CONSTANTES.SUCCESS);
                        return RedirectToAction("Categorias");
                    }
                }
                else if (dto.IdCategoria != 0)
                {
                    if (objBL.update(dto))
                    {
                        createResponseMessage(CONSTANTES.SUCCESS);
                        return RedirectToAction("Categorias");
                    }
                    else
                    {
                        createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_UPDATE_MESSAGE);
                    }

                }
                else
                {
                    createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_INSERT_MESSAGE);
                }
            }
            catch (Exception e)
            {
                if (dto.IdCategoria != 0)
                    createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_UPDATE_MESSAGE);
                else createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_INSERT_MESSAGE);
            }
            TempData["Categoria"] = dto;
            return RedirectToAction("Categoria");
        }

        public ActionResult Movimientos()
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            MovimientoBL objBL = new MovimientoBL();
            return View(objBL.getMovimientos());
        }

        public ActionResult Movimiento(int? id = null, int? idLibro = null)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            //if (!this.isAdministrator()) { return RedirectToAction("Index"); }
            MovimientoBL objBL = new MovimientoBL();
            ViewBag.IdMovimiento = id;
            ViewBag.TiposMovimientos = objBL.getTiposMovimientos(true);
            ViewBag.EstadosMovimientos = objBL.getEstadosMovimientos(true);
            ViewBag.EntidadesResponsables = objBL.getEntidadesResponsables(true);
            ViewBag.NombreCategoria = "Sin Categoría";
            var objSent = TempData["Movimiento"];
            if (objSent != null) { TempData["Movimiento"] = null; return View(objSent); }
            if (id == 0 && idLibro != null)
            {
                MovimientoDTO nuevo = new MovimientoDTO();
                nuevo.IdCuentaBancaria = (int)idLibro;
                nuevo.Fecha = DateTime.Now;
                nuevo.NumeroDocumento = null;
                nuevo.Comentario = "No existe comentario";
                //nuevo.IdEntidadResponsable = 1;
                //nuevo.IdTipoMovimiento = 1;
                //nuevo.IdCategoria = 1;
                //nuevo.IdEstadoMovimiento = 1;
                nuevo.Estado = true;
                nuevo.UsuarioCreacion = getCurrentUser().IdUsuario;
                nuevo.FechaCreacion = DateTime.Now;
                return View(nuevo);
            }
            else
            {
                if (id != null)
                {
                    MovimientoDTO obj = objBL.getMovimiento((int)id);
                    ViewBag.NombreCategoria = objBL.getNombreCategoria(obj.IdCategoria.GetValueOrDefault());
                    return View(obj);
                }
            }
            return View();
        }

        public ActionResult AddMovimiento(MovimientoDTO dto)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            try
            {
                MovimientoBL objBL = new MovimientoBL();
                if (dto.IdMovimiento == 0)
                {
                    if (objBL.add(dto))
                    {
                        //objBL.ActualizarSaldos(dto.IdCuentaBancaria);
                        createResponseMessage(CONSTANTES.SUCCESS);
                        return RedirectToAction("Libro", new { id = dto.IdCuentaBancaria });
                    }
                }
                else if (dto.IdMovimiento != 0)
                {
                    if (objBL.update(dto))
                    {
                        //objBL.ActualizarSaldos(dto.IdCuentaBancaria);
                        createResponseMessage(CONSTANTES.SUCCESS);
                        return RedirectToAction("Libro", new { id = dto.IdCuentaBancaria });
                    }
                    else
                    {
                        createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_UPDATE_MESSAGE);
                    }
                }
                else
                {
                    createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_INSERT_MESSAGE);
                }
            }
            catch (Exception e)
            {
                if (dto.IdMovimiento != 0)
                    createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_UPDATE_MESSAGE);
                else createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_INSERT_MESSAGE);
            }
            TempData["Movimiento"] = dto;
            return RedirectToAction("Movimiento");
        }

        public ActionResult Usuarios()
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            if (!this.isAdministrator()) { return RedirectToAction("Index"); }
            UsuariosBL usuariosBL = new UsuariosBL();
            UsuarioDTO currentUser = getCurrentUser();
            return View(usuariosBL.getUsuarios(currentUser.IdRol));//(CONSTANTES.ROL_RESPONSABLE));
        }

        public ActionResult Usuario(int? id = null)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            UsuarioDTO currentUser = getCurrentUser();
            if (!this.isAdministrator() && id != currentUser.IdUsuario) { return RedirectToAction("Index"); }
            if (id == 1 && !this.isSuperAdministrator()) { return RedirectToAction("Index"); }
            UsuariosBL usuariosBL = new UsuariosBL();
            IList<RolDTO> roles = usuariosBL.getRoles();
            //var rolesList = roles.ToList();
            roles.Insert(0, new RolDTO() { IdRol = 0, Nombre = "Seleccione un Rol" });
            ViewBag.Roles = roles.ToList();//.AsEnumerable();
            var objSent = TempData["Usuario"];
            if (objSent != null) { TempData["Usuario"] = null; return View(objSent); }
            if (id != null)
            {
                UsuarioDTO usuario = usuariosBL.getUsuario((int)id);
                return View(usuario);
            }
            return View();
        }

        public ActionResult AddUser(UsuarioDTO user, string passUser = "", string passChange = "")
        {

            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            UsuarioDTO currentUser = getCurrentUser();
            if (!this.isAdministrator() && user.IdUsuario != currentUser.IdUsuario) { return RedirectToAction("Index"); }
            if (user.IdUsuario == 1 && !this.isSuperAdministrator()) { return RedirectToAction("Index"); }
            try
            {
                UsuariosBL usuariosBL = new UsuariosBL();
                if (user.IdUsuario == 0 && usuariosBL.validateUsuario(user))
                {
                    usuariosBL.add(user);
                    createResponseMessage(CONSTANTES.SUCCESS);
                    return RedirectToAction("Usuarios");
                }
                else if (user.IdUsuario != 0)
                {
                    if (usuariosBL.update(user, passUser, passChange, this.getCurrentUser()))
                    {
                        createResponseMessage(CONSTANTES.SUCCESS);
                        if (user.IdUsuario == this.getCurrentUser().IdUsuario)
                        {
                            System.Web.HttpContext.Current.Session["User"] = usuariosBL.getUsuario(user.IdUsuario);
                            if (!this.getCurrentUser().Active) System.Web.HttpContext.Current.Session["User"] = null;
                        }
                        return RedirectToAction("Usuarios");
                    }
                    else
                    {
                        createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_UPDATE_MESSAGE + "<br>Si está intentando actualizar la contraseña, verifique que ha ingresado la contraseña actual correctamente.");
                    }

                }
                else
                {
                    createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_INSERT_MESSAGE);
                }
            }
            catch
            {
                if (user.IdUsuario != 0)
                    createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_UPDATE_MESSAGE);
                else createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_INSERT_MESSAGE);
            }
            TempData["Usuario"] = user;
            return RedirectToAction("Usuario");
        }

        public ActionResult Entidades()
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            EntidadResponsableBL objBL = new EntidadResponsableBL();
            return View(objBL.getEntidadResponsables());
        }

        public ActionResult Entidad(int? id = null)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            //if (!this.isAdministrator()) { return RedirectToAction("Index"); }
            EntidadResponsableBL objBL = new EntidadResponsableBL();
            ViewBag.IdEntidad = id;
            var objSent = TempData["Entidad"];
            if (objSent != null) { TempData["Entidad"] = null; return View(objSent); }
            if (id != null)
            {
                EntidadResponsableDTO obj = objBL.getEntidadResponsable((int)id);
                return View(obj);
            }
            return View();
        }

        [HttpPost]
        public ActionResult AddEntidad(EntidadResponsableDTO dto)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            try
            {
                EntidadResponsableBL objBL = new EntidadResponsableBL();
                if (dto.IdEntidadResponsable == 0)
                {
                    if (objBL.add(dto))
                    {
                        createResponseMessage(CONSTANTES.SUCCESS);
                        return RedirectToAction("Entidades");
                    }
                }
                else if (dto.IdEntidadResponsable != 0)
                {
                    if (objBL.update(dto))
                    {
                        createResponseMessage(CONSTANTES.SUCCESS);
                        return RedirectToAction("Entidades");
                    }
                    else
                    {
                        createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_UPDATE_MESSAGE);
                    }

                }
                else
                {
                    createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_INSERT_MESSAGE);
                }
            }
            catch (Exception e)
            {
                if (dto.IdEntidadResponsable != 0)
                    createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_UPDATE_MESSAGE);
                else createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_INSERT_MESSAGE);
            }
            TempData["Entidad"] = dto;
            return RedirectToAction("Entidad");
        }

        #region APIs adicionales
        public JsonResult CategoriasJson()
        {
            List<Select2DTO> ListaCategorias = new List<Select2DTO>();

            CategoriaBL objBL = new CategoriaBL();
            var listaCat = CategoriasBucle(null, null);

            return Json(new { listaCat }, JsonRequestBehavior.AllowGet);
        }

        public IList<Select2DTO> CategoriasBucle(int? id = null, IList<CategoriaDTO> lista = null)
        {
            var listaCat = lista;
            if (id == null && lista == null)
            {
                CategoriaBL objBL = new CategoriaBL();
                listaCat = objBL.getCategoriasTree();
            }
            List<Select2DTO> selectTree = new List<Select2DTO>();

            foreach (var item in listaCat)
            {
                if (item.IdCategoria != 1)
                {
                    Select2DTO selectItem = new Select2DTO();
                    selectItem.id = item.IdCategoria;
                    selectItem.text = item.Nombre;
                    if (item.Hijos != null && item.Hijos.Count != 0)
                    {
                        selectItem.children = CategoriasBucle(item.IdCategoria, item.Hijos);
                    }
                    selectTree.Add(selectItem);
                }
            }
            return selectTree;
        }

        #endregion
        public ActionResult ReporteCategorias()
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            CuentaBancariaBL objBL = new CuentaBancariaBL();
            ViewBag.Libros = objBL.getCuentasBancariasBag(true);
            return View();
        }

        [HttpPost]
        public ActionResult GenerarReporte(int? IdCuentaB, DateTime? FechaInicio, DateTime? FechaFin)
        {
            if (IdCuentaB == null || FechaInicio == null || FechaFin == null)
            {
                createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_MESSAGE);
                return RedirectToAction("ReporteCategorias");
            }

            CategoriaBL objBL = new CategoriaBL();
            var data = objBL.getReporteCategorias(IdCuentaB, FechaInicio, FechaFin);

            System.Data.DataTable dt = new System.Data.DataTable();
            dt.Clear();

            dt.Columns.Add("Montos Totales");
            dt.Columns.Add("Categorias");

            foreach (var item in data)
            {
                System.Data.DataRow row = dt.NewRow();
                row["Montos Totales"] = item.MontoTotal.ToString(CultureInfo.InvariantCulture);
                row["Categorias"] = item.Nombre;

                dt.Rows.Add(row);
            }

            GridView gv = new GridView();

            gv.DataSource = dt;
            gv.AllowPaging = false;
            gv.DataBind();

            if (dt.Rows.Count > 0)
            {
                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=Reporte de Categorias.xls");
                Response.ContentType = "application/ms-excel";
                Response.Charset = "";

                StringWriter sw = new StringWriter();
                HtmlTextWriter htw = new HtmlTextWriter(sw);
                gv.RenderControl(htw);
                Response.Output.Write(sw.ToString());
                Response.Flush();
                Response.End();
                htw.Close();
                sw.Close();
            }
            createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_REPORTE_NO_MOVS);
            return RedirectToAction("ReporteCategorias");
            //return View();
        }
    }
}
