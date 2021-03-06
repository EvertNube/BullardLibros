﻿using BullardLibros.Core.BL;
using BullardLibros.Core.DTO;
using BullardLibros.Helpers;
using BullardLibros.Helpers.Razor;
using BullardLibros.Models;
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
using System.Data;
using WebMatrix.WebData;
using System.Text.RegularExpressions;

namespace BullardLibros.Controllers
{
    public class AdminController : Controller
    {
        protected Navbar navbar { get; set; }
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
        private bool isUsuarioInterno()
        {
            if (getCurrentUser().IdRol == 3) return true;
            return false;
        }
        private bool isUsuarioExterno()
        {
            if (getCurrentUser().IdRol == 4) return true;
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
                this.navbar = new Navbar();
                ViewBag.currentUser = user;
                ViewBag.NombreEmpresa = user.nombreEmpresa;
                ViewBag.Title = "NubeBooks";

                ViewBag.EsAdmin = isAdministrator();
                ViewBag.EsSuperAdmin = isSuperAdministrator();
                ViewBag.EsUsuarioInterno = isUsuarioInterno();
                ViewBag.EsUsuarioExterno = isUsuarioExterno();
                ViewBag.IdRol = user.IdRol;

                EmpresaBL empBL = new EmpresaBL();
                ViewBag.Empresas = empBL.getEmpresasViewBag();
            }
            else { ViewBag.EsAdmin = false; ViewBag.EsSuperAdmin = false; }
        }

        public ActionResult Ingresar()
        {
            return View();
        }
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(string UserName, string codigoEmpresa)
        {
            UsuariosBL objBL = new UsuariosBL();

            UsuarioDTO usuario = new UsuarioDTO() { Cuenta = UserName, Email = UserName, codigoEmpresa = codigoEmpresa };
            usuario = objBL.getUserByAcountOrEmail(usuario);

            if (usuario == null)
            {
                createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_RECOVERY_PASSWORD);
                return RedirectToAction("ForgotPassword", "Admin");
            }
            else
            {
                //if (objBL.recoverPasswordNew(usuario))
                usuario = objBL.generateTokenRecoverPassword(usuario);
                if (usuario.Token != null)
                {
                    string link = "<a href='" + this.Url.Action("ResetPassword", "Admin", new { rt = usuario.Token, emp = usuario.codigoEmpresa }, this.Request.Url.Scheme) + "'>Reset Password</a>";
                    objBL.SendMailResetPassword(usuario, link);
                    createResponseMessage(CONSTANTES.SUCCESS, CONSTANTES.SUCCESS_MESSAGE_FOR_RECOVERY_PASSWORD);
                }
                else
                {
                    createResponseMessage(CONSTANTES.ERROR, "<strong>Hubo un error al recuperar la contraseña.</strong>");
                    return RedirectToAction("ForgotPassword", "Admin");
                }
            }

            return RedirectToAction("Ingresar", "Admin");
        }
        [AllowAnonymous]
        public ActionResult ResetPassword(string rt, string emp)
        {
            if(rt == null || emp == null)
            {
                return RedirectToAction("Ingresar", "Admin");
            }

            ResetPasswordDTO obj = new ResetPasswordDTO();
            obj.rt = rt;
            obj.emp = emp;
            return View(obj);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetPasswordDTO obj)
        {
            if(obj.rt == null || obj.emp == null || obj.Password == null || obj.Password == "")
            {
                return RedirectToAction("Ingresar", "Admin");
            }
            if(obj.Password != obj.ConfirmPassword)
            {
                createResponseMessage(CONSTANTES.ERROR, "<strong>Las contraseñas ingresadas tienen que coincidir.</strong>");
                return View();
            }
            UsuariosBL objBL = new UsuariosBL();
            UsuarioDTO usuario = new UsuarioDTO() { Token = obj.rt, codigoEmpresa = obj.emp, Pass = obj.Password };
            
            if(objBL.resetPasswordByTokenAndEmp(usuario))
            {
                createResponseMessage(CONSTANTES.SUCCESS, CONSTANTES.SUCCESS_PASSWORD_CHANGE);
                return RedirectToAction("Ingresar", "Admin");
            }
            createResponseMessage(CONSTANTES.ERROR, "<strong>Usted no puede realizar esta acción o hubo un error al intentar cambiar la contraseña.</strong>");
            return View();
        }


        [HttpPost]
        public ActionResult Login(UsuarioDTO user)
        {
            UsuariosBL usuariosBL = new UsuariosBL();
            if (usuariosBL.isValidUser(user))
            {
                System.Web.HttpContext.Current.Session["User"] = usuariosBL.getUsuarioByCuenta(user);
                return RedirectToAction("Index");
            }
            createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_LOGIN);
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
            /*
            EmpresaBL empBL = new EmpresaBL();
            
            if(isSuperAdministrator())
            {
                MenuNavBarSelected(0);
                
                List<EmpresaDTO> listaEmpresas = new List<EmpresaDTO>();
                listaEmpresas = empBL.getEmpresas();

                return View(listaEmpresas);
            }

            MenuNavBarSelected(1);

            ViewBag.TipoCambio = empBL.getEmpresa((int)getCurrentUser().IdEmpresa).TipoCambio;

            CuentaBancariaBL objBL = new CuentaBancariaBL();
            List<CuentaBancariaDTO> listaLibros = new List<CuentaBancariaDTO>();
            listaLibros = objBL.getCuentasBancariasEnEmpresa(getCurrentUser().IdEmpresa);
            ViewBag.TotalSoles = DameTotalSoles(listaLibros);
            ViewBag.TotalDolares = DameTotalDolares(listaLibros);
            ViewBag.TotalConsolidado = DameTotalConsolidado(listaLibros);
            return View("Libros", listaLibros);*/
            return RedirectToAction("Libros", "Admin");
        }
        public ActionResult Empresa(int? id = null)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            if (!this.isSuperAdministrator()) { return RedirectToAction("Index"); }
            MenuNavBarSelected(0);
            UsuarioDTO currentUser = getCurrentUser();

            EmpresaBL objBL = new EmpresaBL();
            ViewBag.IdEmpresa = id;

            var objSent = TempData["Empresa"];
            if (objSent != null) { TempData["Empresa"] = null; return View(objSent); }
            if (id != null)
            {
                EmpresaDTO obj = objBL.getEmpresa((int)id);
                if (obj == null) return RedirectToAction("Index");
                return View(obj);
            }
            return View();
        }

        [HttpPost]
        public ActionResult AddEmpresa(EmpresaDTO dto)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            try
            {
                EmpresaBL objBL = new EmpresaBL();
                if (dto.IdEmpresa == 0)
                {
                    if (objBL.add(dto))
                    {
                        createResponseMessage(CONSTANTES.SUCCESS);
                        return RedirectToAction("Index");
                    }
                }
                else if (dto.IdEmpresa != 0)
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
                if (dto.IdEmpresa != 0)
                    createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_UPDATE_MESSAGE);
                else createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_INSERT_MESSAGE);
            }
            TempData["Empresa"] = dto;
            return RedirectToAction("Empresa");
        }

        public ActionResult Libros(int? idTipoCuenta = null)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            ViewBag.Title += " - Libros";
            MenuNavBarSelected(1);

            UsuarioDTO miUsuario = getCurrentUser();

            CuentaBancariaBL objBL = new CuentaBancariaBL();
            ViewBag.lstTipoCuentas = objBL.getTipoDeCuentas();
            ViewBag.idTipoCuenta = idTipoCuenta;
            List<CuentaBancariaDTO> listaLibros = new List<CuentaBancariaDTO>();

            if (miUsuario.IdEmpresa != 0)
            {
                EmpresaBL empBL = new EmpresaBL();
                Decimal miTipoCambio = empBL.getEmpresa((int)miUsuario.IdEmpresa).TipoCambio;

                listaLibros = objBL.getCuentasBancariasEnEmpresa(miUsuario.IdEmpresa);
                ViewBag.TotalSoles = DameTotalSoles(listaLibros);
                ViewBag.TotalDolares = DameTotalDolares(listaLibros);

                ViewBag.TotalConsolidado = DameTotalConsolidado(listaLibros, miTipoCambio);
                ViewBag.TipoCambio = miTipoCambio;
            }
            else
            {
                ViewBag.TotalSoles = 0.0;
                ViewBag.TotalDolares = 0.0;
                ViewBag.TotalConsolidado = 0.0;
                ViewBag.TipoCambio = 1.0;
            }

            return View("Libros", listaLibros);
        }

        public ActionResult Libro(int? id = null, int? idTipoCuenta = null, string sortOrder = null, string currentFilter = null, string searchString = null, int? page = null)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            ViewBag.Title += " - Libro";
            MenuNavBarSelected(1);

            UsuarioDTO miUsuario = getCurrentUser();

            CuentaBancariaBL objBL = new CuentaBancariaBL();
            ViewBag.IdCuentaBancaria = id;
            ViewBag.Monedas = objBL.getMonedasBag(false);
            var objSent = TempData["Libro"];
            if (objSent != null) { TempData["Libro"] = null; return View(objSent); }

            CuentaBancariaDTO obj;
            if (id != null && id != 0)
            {
                //Actualizar Saldo Disponible
                objBL.updateSaldos((int)id);
                obj = objBL.getCuentaBancariaEnEmpresa(miUsuario.IdEmpresa, (int)id);
                if (obj == null) return RedirectToAction("Index");
                if (obj.IdEmpresa != miUsuario.IdEmpresa) return RedirectToAction("Index");

                obj.listaMovimientoPL = BusquedaYPaginado_Movimiento(obj.listaMovimiento, sortOrder, currentFilter, searchString, page);
                return View(obj);
            }

            obj = new CuentaBancariaDTO();
            obj.FechaConciliacion = DateTime.Now;
            obj.IdEmpresa = miUsuario.IdEmpresa;
            if (idTipoCuenta != null && idTipoCuenta != 0) obj.IdTipoCuenta = idTipoCuenta.GetValueOrDefault();

            return View(obj);
        }
        private IPagedList<MovimientoDTO> BusquedaYPaginado_Movimiento(IList<MovimientoDTO> lista, string sortOrder, string currentFilter, string searchString, int? page)
        {
            if (!String.IsNullOrEmpty(searchString))
            { searchString = searchString.ToLower(); }
            ViewBag.CurrentSort = sortOrder;

            ViewBag.vbFecha = sortOrder == "Fecha" ? "Fecha_desc" : "Fecha";
            ViewBag.vbTipo = sortOrder == "Tipo" ? "Tipo_desc" : "Tipo";
            ViewBag.vbDetalle = sortOrder == "Detalle" ? "Detalle_desc" : "Detalle";
            ViewBag.vbMonto = sortOrder == "Monto" ? "Monto_desc" : "Monto";
            ViewBag.vbCategoria = sortOrder == "Categoria" ? "Categoria_desc" : "Categoria";
            ViewBag.vbEntidad = sortOrder == "Entidad" ? "Entidad_desc" : "Entidad";
            ViewBag.vbDocumento = sortOrder == "Documento" ? "Documento_desc" : "Documento";
            ViewBag.vbUsuario = sortOrder == "Usuario" ? "Usuario_desc" : "Usuario";
            ViewBag.vbEstado = sortOrder == "Estado" ? "Estado_desc" : "Estado";

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            string tipoDato = "cadena";
            DateTime pTiempo;
            if (DateTime.TryParse(searchString, out pTiempo))
            {
                tipoDato = "tiempo";
                pTiempo = Convert.ToDateTime(searchString);
            }

            Decimal pDecimal;
            if (Decimal.TryParse(searchString, out pDecimal))
            {
                tipoDato = "numerico";
                pDecimal = Convert.ToDecimal(searchString);
            }

            if (!String.IsNullOrEmpty(searchString))
            {
                IList<MovimientoDTO> listaP;
                listaP = lista.Where(s => (s.NroOperacion.ToLower() ?? "").Contains(searchString)
                        || (s.NombreCategoria.ToLower() ?? "").Contains(searchString)
                        || (s.NombreEntidadR.ToLower() ?? "").Contains(searchString)
                        || (s.NumeroDocumento.ToLower() ?? "").Contains(searchString)
                        || (s.NombreUsuario.ToLower() ?? "").Contains(searchString)
                        ).ToList();

                switch (tipoDato)
                {
                    case "tiempo":
                        lista = lista.Where(s => DateTime.Compare(s.Fecha, pTiempo) <= 0).ToList();
                        lista = lista.Union(listaP).ToList();
                        break;
                    case "numerico":
                        lista = lista.Where(s => s.Monto.ToString().Contains(pDecimal.ToString())).ToList();
                        lista = lista.Union(listaP).ToList();
                        break;
                    default:
                        lista = listaP;
                        break;
                }
            }

            switch (sortOrder)
            {
                case "Fecha":
                    lista = lista.OrderBy(s => s.Fecha).ToList();
                    break;
                case "Tipo":
                    lista = lista.OrderBy(s => s.IdTipoMovimiento).ToList();
                    break;
                case "Detalle":
                    lista = lista.OrderBy(s => s.NroOperacion).ToList();
                    break;
                case "Monto":
                    lista = lista.OrderBy(s => s.Monto).ToList();
                    break;
                case "Categoria":
                    lista = lista.OrderBy(s => s.NombreCategoria).ToList();
                    break;
                case "Entidad":
                    lista = lista.OrderBy(s => s.NombreEntidadR).ToList();
                    break;
                case "Documento":
                    lista = lista.OrderBy(s => s.NumeroDocumento).ToList();
                    break;
                case "Usuario":
                    lista = lista.OrderBy(s => s.NombreUsuario).ToList();
                    break;
                case "Estado":
                    lista = lista.OrderBy(s => s.IdEstadoMovimiento).ToList();
                    break;
                case "Fecha_desc":
                    lista = lista.OrderByDescending(s => s.Fecha).ToList();
                    break;
                case "Tipo_desc":
                    lista = lista.OrderByDescending(s => s.IdTipoMovimiento).ToList();
                    break;
                case "Detalle_desc":
                    lista = lista.OrderByDescending(s => s.NroOperacion).ToList();
                    break;
                case "Monto_desc":
                    lista = lista.OrderByDescending(s => s.Monto).ToList();
                    break;
                case "Categoria_desc":
                    lista = lista.OrderByDescending(s => s.NombreCategoria).ToList();
                    break;
                case "Entidad_desc":
                    lista = lista.OrderByDescending(s => s.NombreEntidadR).ToList();
                    break;
                case "Documento_desc":
                    lista = lista.OrderByDescending(s => s.NumeroDocumento).ToList();
                    break;
                case "Usuario_desc":
                    lista = lista.OrderByDescending(s => s.NombreUsuario).ToList();
                    break;
                case "Estado_desc":
                    lista = lista.OrderByDescending(s => s.IdEstadoMovimiento).ToList();
                    break;
            }

            int pageSize = 50;
            int pageNumber = (page ?? 1);

            return lista.ToPagedList(pageNumber, pageSize);
        }
        [HttpPost]
        public ActionResult AddLibro(CuentaBancariaDTO dto)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            if (getCurrentUser().IdRol == 4) { return RedirectToAction("Libros", "Admin"); }
            try
            {
                int TipoCuenta = 1; //Por defecto tipo de comprobante Ingreso
                if (dto != null) { TipoCuenta = dto.IdTipoCuenta.GetValueOrDefault(); }

                CuentaBancariaBL objBL = new CuentaBancariaBL();
                if (dto.IdCuentaBancaria == 0)
                {
                    if (objBL.add(dto))
                    {
                        createResponseMessage(CONSTANTES.SUCCESS);
                        return RedirectToAction("Libros", "Admin", new { idTipoCuenta = TipoCuenta });
                    }
                }
                else if (dto.IdCuentaBancaria != 0)
                {
                    if (objBL.update(dto))
                    {
                        createResponseMessage(CONSTANTES.SUCCESS);
                        return RedirectToAction("Libros", "Admin", new { idTipoCuenta = TipoCuenta });
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
            if (!isAdministrator()) { return RedirectToAction("Index"); }

            ViewBag.Title += " - Categorias";

            MenuNavBarSelected(4, 5);
            EmpresaBL empBL = new EmpresaBL();

            UsuarioDTO miUsuario = getCurrentUser();
            EmpresaDTO empresa = empBL.getEmpresa(miUsuario.IdEmpresa);
            ViewBag.IdPeriodo = empresa.IdPeriodo.GetValueOrDefault();
            ViewBag.SimboloMoneda = empresa.SimboloMoneda;

            CategoriaBL objBL = new CategoriaBL();
            ViewBag.Periodos = objBL.GetPeriodosEnEmpresaViewBag(miUsuario.IdEmpresa);
            List<CategoriaDTO> listaCategorias = new List<CategoriaDTO>();
            if (miUsuario.IdEmpresa > 0)
            {
                listaCategorias = objBL.getCategoriasTreeEnEmpresa(miUsuario.IdEmpresa);
            }

            return View(listaCategorias);
        }
        public ActionResult Categoria(int? id = null, int? idPadre = null)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            if (!this.isAdministrator()) { return RedirectToAction("Index"); }

            ViewBag.Title += " - Categoría";

            MenuNavBarSelected(4, 5);
            UsuarioDTO miUsuario = getCurrentUser();

            CategoriaBL objBL = new CategoriaBL();
            ViewBag.IdCategoria = id;

            ViewBag.Categorias = CategoriasBucle(null, null);

            ViewBag.NombreCategoria = "Sin Categoría";
            var objSent = TempData["Categoria"];
            if (objSent != null) { TempData["Categoria"] = null; return View(objSent); }

            CategoriaDTO obj;
            if (id != null || id == 0)
            {
                if (idPadre != null)
                {
                    CategoriaDTO objp = new CategoriaDTO();

                    objp.IdCategoria = 0;
                    objp.IdCategoriaPadre = idPadre;
                    objp.Orden = objBL.getUltimoHijo(idPadre.GetValueOrDefault());
                    objp.IdEmpresa = miUsuario.IdEmpresa;

                    ViewBag.NombreCategoria = objBL.getNombreCategoria(objp.IdCategoriaPadre.GetValueOrDefault());
                    return View(objp);
                }
                obj = objBL.getCategoria((int)id);
                if (obj == null) return RedirectToAction("Categorias");
                if (obj.IdEmpresa != miUsuario.IdEmpresa) return RedirectToAction("Categorias");

                ViewBag.NombreCategoria = objBL.getNombreCategoria(obj.IdCategoriaPadre.GetValueOrDefault());
                return View(obj);
            }
            obj = new CategoriaDTO();
            obj.IdEmpresa = miUsuario.IdEmpresa;

            return View(obj);
        }
        [HttpPost]
        public ActionResult AddCategoria(CategoriaDTO dto)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            if (!this.isAdministrator()) { return RedirectToAction("Index"); }

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
                    EmpresaBL empBL = new EmpresaBL();
                    int vPeriodo = empBL.getEmpresa(getCurrentUser().IdEmpresa).IdPeriodo.GetValueOrDefault();

                    if (objBL.update(dto, vPeriodo))
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

            ViewBag.Title += " - Movimientos";

            MovimientoBL objBL = new MovimientoBL();
            return View(objBL.getMovimientos());
        }
        public ActionResult Movimiento(int? id = null, int? idLibro = null)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            ViewBag.Title += " - Movimiento";
            MenuNavBarSelected(1);
            UsuarioDTO miUsuario = getCurrentUser();

            MovimientoBL objBL = new MovimientoBL();
            ViewBag.IdMovimiento = id;
            ViewBag.EstadosMovimientos = objBL.getEstadosMovimientos(false);

            CuentaBancariaBL objCuentaBL = new CuentaBancariaBL();
            CuentaBancariaDTO objLibro = objCuentaBL.getCuentaBancariaSoloEnEmpresa(miUsuario.IdEmpresa, idLibro.GetValueOrDefault());
            if (objLibro == null) { return RedirectToAction("Index", "Admin"); }

            ViewBag.IdTipoCuenta = objLibro.IdTipoCuenta;
            ViewBag.lstFormaMovs = ViewBag.IdTipoCuenta != 2 ? objBL.getListaFormaDeMovimientos() : objBL.getListaFormaDeMovimientosBasic();

            ViewBag.EntidadesResponsables = objBL.getEntidadesResponsablesEnEmpresa(miUsuario.IdEmpresa, false);
            ViewBag.lstTiposDeDocumento = objBL.getListaTiposDeDocumentoVB(true);
            ViewBag.NombreCategoria = "Sin Categoría";
            ViewBag.Categorias = CategoriasBucle(null, null);
            ViewBag.Comprobantes = objBL.getComprobantesPendientesEnEmpresa(miUsuario.IdEmpresa);

            var objSent = TempData["Movimiento"];
            if (objSent != null) { TempData["Movimiento"] = null; return View(objSent); }
            if (id == 0 && idLibro != null)
            {
                MovimientoDTO nuevo = new MovimientoDTO();
                nuevo.IdCuentaBancaria = (int)idLibro;
                nuevo.Fecha = DateTime.Now;
                nuevo.TipoCambio = (new EmpresaBL()).getEmpresa(miUsuario.IdEmpresa).TipoCambio;
                nuevo.NumeroDocumento = null;
                nuevo.Comentario = "No existe comentario";
                nuevo.Estado = true;
                nuevo.UsuarioCreacion = miUsuario.IdUsuario;
                nuevo.FechaCreacion = DateTime.Now;
                return View(nuevo);
            }
            else
            {
                if (id != null)
                {
                    MovimientoDTO obj = objBL.getMovimiento((int)id);
                    if (obj == null) return RedirectToAction("Libro", "Admin", new { id = objLibro.IdCuentaBancaria });
                    if (obj.IdCuentaBancaria != objLibro.IdCuentaBancaria) return RedirectToAction("Libro", "Admin", new { id = objLibro.IdCuentaBancaria });
                    
                    CuentaBancariaDTO objLibroMov = objCuentaBL.getCuentaBancariaEnEmpresa(miUsuario.IdEmpresa, obj.IdCuentaBancaria);
                    if (objLibroMov == null) return RedirectToAction("Index", "Admin");
                    if (objLibroMov.IdEmpresa != miUsuario.IdEmpresa) return RedirectToAction("Index", "Admin");

                    obj.UsuarioCreacion = miUsuario.IdUsuario;
                    ViewBag.NombreCategoria = objBL.getNombreCategoria(obj.IdCategoria.GetValueOrDefault());
                    return View(obj);
                }
            }
            return View();
        }
        public ActionResult AddMovimiento(MovimientoDTO dto)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            if (getCurrentUser().IdRol == 4) { return RedirectToAction("Libro", new { id = dto.IdCuentaBancaria }); }
            try
            {
                MovimientoBL objBL = new MovimientoBL();
                MovimientoDTO dtoAnterior = objBL.getMovimiento(dto.IdMovimiento);
                if(dto.IdComprobante != null && dto.cmpMontoPendiente != null)
                {
                    if(dto.cmpMontoPendiente < 0)
                    {
                        createResponseMessage(CONSTANTES.ERROR, "<strong>Error.</strong> No se puede pagar un monto mayor al monto pendiente");
                        dto.Monto = dto.IdMovimiento != 0 ? dtoAnterior.Monto : 0;
                        TempData["Movimiento"] = dto;
                        return RedirectToAction("Movimiento", new { id = 0, idLibro = dto.IdCuentaBancaria });
                    }
                    else if(dto.Monto == dto.cmpMontoPendiente)
                    {
                        ActualizarEjecucionComprobante(dto.IdComprobante.GetValueOrDefault(), true);
                    }
                    else
                    {
                        ActualizarEjecucionComprobante(dto.IdComprobante.GetValueOrDefault(), dto.cmpCancelado);
                    }
                }
                if (dto.IdMovimiento == 0)
                {
                    if (objBL.add(dto))
                    {
                        //objBL.ActualizarSaldos(dto.IdCuentaBancaria);
                        createResponseMessage(CONSTANTES.SUCCESS);
                        return RedirectToAction("Libro", new { id = dto.IdCuentaBancaria, page = TempData["PagMovs"] });
                    }
                }
                else if (dto.IdMovimiento != 0)
                {
                    if (objBL.update(dto))
                    {
                        //Si en la actualizacion se cambio el IdComprobante
                        if (dtoAnterior.IdComprobante != null && dtoAnterior.IdComprobante != dto.IdComprobante)
                        {
                            ActualizarEjecucionComprobante(dtoAnterior.IdComprobante.GetValueOrDefault(), false);
                        }

                        createResponseMessage(CONSTANTES.SUCCESS);
                        return RedirectToAction("Libro", new { id = dto.IdCuentaBancaria, page = TempData["PagMovs"] });
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

        [HttpPost]
        public ActionResult DeleteMovimiento(int id, int idCuentaBancaria)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            if (getCurrentUser().IdRol == 4) { return RedirectToAction("Libro", "Admin", new { id = idCuentaBancaria }); }

            try
            {
                MovimientoBL objBL = new MovimientoBL();
                if (objBL.delete(id))
                {
                    createResponseMessage(CONSTANTES.SUCCESS, CONSTANTES.SUCCESS_DELETE);
                }
                else
                {
                    createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_DELETE);
                }
            }
            catch (Exception e)
            {
                createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_NO_DELETE);
                throw;
            }
            return RedirectToAction("Libro", "Admin", new { id = idCuentaBancaria });
        }
        public ActionResult Usuarios()
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            if (!this.isAdministrator()) { return RedirectToAction("Index"); }
            ViewBag.Title += " - Usuarios";
            MenuNavBarSelected(4, 7);

            UsuarioDTO currentUser = getCurrentUser();

            UsuariosBL usuariosBL = new UsuariosBL();
            List<UsuarioDTO> listaUsuarios = new List<UsuarioDTO>();
            if (currentUser.IdEmpresa > 0)
            {
                listaUsuarios = usuariosBL.getUsuariosEnEmpresa(currentUser.IdEmpresa, currentUser.IdRol);
            }

            return View(listaUsuarios);
        }
        public ActionResult Usuario(int? id = null)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            ViewBag.Title += " - Usuario";
            MenuNavBarSelected(4, 7);

            UsuarioDTO currentUser = getCurrentUser();
            UsuariosBL usuariosBL = new UsuariosBL();

            if (!this.isAdministrator() && id != currentUser.IdUsuario) { return RedirectToAction("Index"); }
            if (!this.isSuperAdministrator() && usuariosBL.getUsuario(id.GetValueOrDefault()).IdRol == 1) { return RedirectToAction("Usuarios"); }

            //ViewBag.vbRls = usuariosBL.getAllRolesViewBag(false);
            ViewBag.vbRls = usuariosBL.getRolesDown(currentUser.IdRol);

            var objSent = TempData["Usuario"];
            if (objSent != null) { TempData["Usuario"] = null; return View(objSent); }
            UsuarioDTO usuario;
            if (id != null)
            {
                usuario = usuariosBL.getUsuarioEnEmpresa(currentUser.IdEmpresa, id.GetValueOrDefault());
                if (usuario == null) return RedirectToAction("Usuarios");
                if (usuario.IdEmpresa != currentUser.IdEmpresa) return RedirectToAction("Usuarios");

                return View(usuario);
            }
            usuario = new UsuarioDTO();
            usuario.IdEmpresa = currentUser.IdEmpresa;
            return View(usuario);
        }
        [HttpPost]
        public ActionResult AddUser(UsuarioDTO user, string passUser = "", string passChange = "")
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            UsuarioDTO currentUser = getCurrentUser();
            if (!this.isAdministrator() && user.IdUsuario != currentUser.IdUsuario) { return RedirectToAction("Index"); }
            if (user.IdRol == 1 && !this.isSuperAdministrator()) { return RedirectToAction("Index"); }
            try
            {
                UsuariosBL usuariosBL = new UsuariosBL();
                /*if (usuariosBL.validateUsuario(user))
                {
                    createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_INSERT_DUPLICATE_USER);
                    TempData["Usuario"] = user;
                    return RedirectToAction("Usuario");
                }*/
                if (user.IdUsuario == 0 && usuariosBL.validateUsuario(user))
                {
                    if (!this.isSuperAdministrator()) { return RedirectToAction("Index"); }

                    usuariosBL.add(user);
                    createResponseMessage(CONSTANTES.SUCCESS);
                    return RedirectToAction("Usuarios");
                }
                else if (user.IdUsuario != 0 && usuariosBL.validateUsuarioNoDuplicado(user))
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
                        TempData["Usuario"] = user;
                        return RedirectToAction("Usuario", new { id = user.IdUsuario });
                    }

                }
                else
                {
                    createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_INSERT_DUPLICATE_USER);
                }
            }
            catch
            {
                if (user.IdUsuario != 0)
                    createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_UPDATE_MESSAGE);
                else createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_INSERT_ACCOUNT);
            }
            TempData["Usuario"] = user;
            return RedirectToAction("Usuario");
        }
        public ActionResult Entidades(int? idTipoEntidad = null)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            //if (!isAdministrator()) { return RedirectToAction("Index"); }
            //Solo usuario Externo no tiene acceso a esto
            if (isUsuarioExterno()) { return RedirectToAction("Index"); }

            ViewBag.Title += " - Entidades";
            MenuNavBarSelected(4, 3);
            UsuarioDTO currentUser = getCurrentUser();

            EntidadResponsableBL objBL = new EntidadResponsableBL();
            ViewBag.idTipoEntidad = idTipoEntidad;
            List<EntidadResponsableDTO> listaEntidades = new List<EntidadResponsableDTO>();
            ViewBag.lstTipoEntidades = objBL.getTipoDeEntidades();

            if (currentUser.IdEmpresa > 0)
            {
                listaEntidades = objBL.getEntidadResponsablesEnEmpresa(currentUser.IdEmpresa);
            }
            return View(listaEntidades);
        }
        public ActionResult Entidad(int? id = null, int? idTipoEntidad = null)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            if (isUsuarioExterno()) { return RedirectToAction("Index"); }
            ViewBag.Title += " - Entidad";
            MenuNavBarSelected(4, 3);

            UsuarioDTO currentUser = getCurrentUser();

            EntidadResponsableBL objBL = new EntidadResponsableBL();
            ViewBag.TipoIdentificacion = objBL.getTiposDeIdentificaciones();

            var objSent = TempData["Entidad"];
            if (objSent != null) { TempData["Entidad"] = null; return View(objSent); }

            EntidadResponsableDTO obj;
            if (id != null && id != 0)
            {
                obj = objBL.getEntidadResponsableEnEmpresa((int)currentUser.IdEmpresa, (int)id);
                if (obj == null) return RedirectToAction("Entidades");
                if (obj.IdEmpresa != currentUser.IdEmpresa) return RedirectToAction("Entidades");
                return View(obj);
            }
            obj = new EntidadResponsableDTO();
            obj.IdEmpresa = currentUser.IdEmpresa;
            if (idTipoEntidad != null && idTipoEntidad != 0) obj.IdTipoEntidad = idTipoEntidad;

            return View(obj);
        }
        [HttpPost]
        public ActionResult AddEntidad(EntidadResponsableDTO dto)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            try
            {
                int TipoEntidad = 1; //Por defecto tipo de comprobante Ingreso
                if (dto != null) { TipoEntidad = dto.IdTipoEntidad.GetValueOrDefault(); }
                EntidadResponsableBL objBL = new EntidadResponsableBL();
                if (dto.IdEntidadResponsable == 0)
                {
                    if (objBL.add(dto))
                    {
                        createResponseMessage(CONSTANTES.SUCCESS);
                        return RedirectToAction("Entidades", "Admin", new { idTipoEntidad = TipoEntidad });
                    }
                }
                else if (dto.IdEntidadResponsable != 0)
                {
                    if (objBL.update(dto))
                    {
                        createResponseMessage(CONSTANTES.SUCCESS);
                        return RedirectToAction("Entidades", "Admin", new { idTipoEntidad = TipoEntidad });
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
        public ActionResult Comprobantes(int? idTipoComprobante = null, string sortOrder = null, string currentFilter = null, string searchString = null, int? page = null)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            ViewBag.Title += " - Comprobantes";
            MenuNavBarSelected(2);

            UsuarioDTO currentUser = getCurrentUser();

            ComprobanteBL objBL = new ComprobanteBL();
            List<ComprobanteDTO> listaIngresos = new List<ComprobanteDTO>();
            List<ComprobanteDTO> listaEgresos = new List<ComprobanteDTO>();
            ViewBag.lstTipoComprobantes = objBL.getTipoDeComprobantes();
            ViewBag.idTipoComprobante = idTipoComprobante;

            if (currentUser.IdEmpresa > 0)
            {
                listaIngresos = objBL.getComprobantesEnEmpresaPorTipo(currentUser.IdEmpresa, 1);
                listaEgresos = objBL.getComprobantesEnEmpresaPorTipo(currentUser.IdEmpresa, 2);
                List<IPagedList<ComprobanteDTO>> matrix = new List<IPagedList<ComprobanteDTO>>();
                matrix.Add(BusquedaYPaginado_Comprobantes(listaIngresos, sortOrder, currentFilter, searchString, page));
                matrix.Add(BusquedaYPaginado_Comprobantes(listaEgresos, sortOrder, currentFilter, searchString, page));
                return View(matrix);
            }
            return View();
        }

        public ActionResult ComprobantesIngreso(int? idTipoComprobante = 1)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            ViewBag.Title += " - Comprobantes de Ingreso";

            MenuNavBarSelected(2);

            UsuarioDTO user = getCurrentUser();

            ComprobanteBL objBL = new ComprobanteBL();
            int tipo = 1; //Ingresos
            ViewBag.idTipoComprobante = tipo;

            if (user.IdEmpresa > 0)
            {
                List<ComprobanteDTO> lista = objBL.getComprobantesEnEmpresaPorTipo(user.IdEmpresa, tipo);
                return View(lista);
            }
            return View();
        }

        public ActionResult ComprobantesEgreso()
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            ViewBag.Title += " - Comprobantes de Egreso";

            MenuNavBarSelected(2);

            UsuarioDTO user = getCurrentUser();

            ComprobanteBL objBL = new ComprobanteBL();
            int tipo = 2; //Egresos
            ViewBag.idTipoComprobante = tipo;

            if(user.IdEmpresa > 0)
            {
                List<ComprobanteDTO> lista = objBL.getComprobantesEnEmpresaPorTipo(user.IdEmpresa, tipo);
                return View(lista);
            }
            return View();
        }

        private IPagedList<ComprobanteDTO> BusquedaYPaginado_Comprobantes(IList<ComprobanteDTO> lista, string sortOrder, string currentFilter, string searchString, int? page)
        {
            if (!String.IsNullOrEmpty(searchString))
            { searchString = searchString.ToLower(); }
            ViewBag.CurrentSort = sortOrder;

            ViewBag.vbFecha = sortOrder == "Fecha" ? "Fecha_desc" : "Fecha";
            ViewBag.vbDocumento = sortOrder == "Documento" ? "Documento_desc" : "Documento";
            ViewBag.vbNumero = sortOrder == "Numero" ? "Numero_desc" : "Numero";
            ViewBag.vbEntidad = sortOrder == "Entidad" ? "Entidad_desc" : "Entidad";
            ViewBag.vbProyecto = sortOrder == "Proyecto" ? "Proyecto_desc" : "Proyecto";
            ViewBag.vbMontoSinIGV = sortOrder == "MontoSinIGV" ? "MontoSinIGV_desc" : "MontoSinIGV";
            ViewBag.vbCategoria = sortOrder == "Categoria" ? "Categoria_desc" : "Categoria";
            ViewBag.vbFechaFin = sortOrder == "FechaFin" ? "FechaFin_desc" : "FechaFin";
            ViewBag.vbUsuario = sortOrder == "Usuario" ? "Usuario_desc" : "Usuario";
            ViewBag.vbEstado = sortOrder == "Estado" ? "Estado_desc" : "Estado";

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            string tipoDato = "cadena";
            DateTime pTiempo;
            if (DateTime.TryParse(searchString, out pTiempo))
            {
                tipoDato = "tiempo";
                pTiempo = Convert.ToDateTime(searchString);
            }

            Decimal pDecimal;
            if (Decimal.TryParse(searchString, out pDecimal))
            {
                tipoDato = "numerico";
                pDecimal = Convert.ToDecimal(searchString);
            }

            if (!String.IsNullOrEmpty(searchString))
            {
                IList<ComprobanteDTO> listaP;
                listaP = lista.Where(s => (s.NombreTipoDocumento.ToLower() ?? "").Contains(searchString)
                        || (s.NombreCategoria.ToLower() ?? "").Contains(searchString)
                        || (s.NroDocumento.ToLower() ?? "").Contains(searchString)
                        || (s.NombreEntidad.ToLower() ?? "").Contains(searchString)
                        || (s.NombreUsuario.ToLower() ?? "").Contains(searchString)
                        || (s.NombreProyecto.ToLower() ?? "").Contains(searchString)
                        ).ToList();

                switch (tipoDato)
                {
                    case "tiempo":
                        lista = lista.Where(s => DateTime.Compare(s.FechaEmision, pTiempo) <= 0 || DateTime.Compare(s.FechaConclusion.GetValueOrDefault(), pTiempo) <= 0).ToList();
                        lista = lista.Union(listaP).ToList();
                        break;
                    case "numerico":
                        lista = lista.Where(s => s.MontoSinIGV.ToString().Contains(pDecimal.ToString())).ToList();
                        lista = lista.Union(listaP).ToList();
                        break;
                    default:
                        lista = listaP;
                        break;
                }
            }

            switch (sortOrder)
            {
                case "Documento":
                    lista = lista.OrderBy(s => s.NombreTipoDocumento).ToList();
                    break;
                case "Numero":
                    lista = lista.OrderBy(s => s.NroDocumento).ToList();
                    break;
                case "Categoria":
                    lista = lista.OrderBy(s => s.NombreCategoria).ToList();
                    break;
                case "Entidad":
                    lista = lista.OrderBy(s => s.NombreEntidad).ToList();
                    break;
                case "MontoSinIGV":
                    lista = lista.OrderBy(s => s.MontoSinIGV).ToList();
                    break;
                case "Usuario":
                    lista = lista.OrderBy(s => s.NombreUsuario).ToList();
                    break;
                case "Fecha":
                    lista = lista.OrderBy(s => s.FechaEmision).ToList();
                    break;
                case "FechaFin":
                    lista = lista.OrderBy(s => s.FechaConclusion).ToList();
                    break;
                case "Estado":
                    lista = lista.OrderBy(s => s.Ejecutado).ToList();
                    break;
                case "Documento_desc":
                    lista = lista.OrderByDescending(s => s.NombreTipoDocumento).ToList();
                    break;
                case "Numero_desc":
                    lista = lista.OrderByDescending(s => s.NroDocumento).ToList();
                    break;
                case "Categoria_desc":
                    lista = lista.OrderByDescending(s => s.NombreCategoria).ToList();
                    break;
                case "Entidad_desc":
                    lista = lista.OrderByDescending(s => s.NombreEntidad).ToList();
                    break;
                case "MontoSinIGV_desc":
                    lista = lista.OrderByDescending(s => s.MontoSinIGV).ToList();
                    break;
                case "Usuario_desc":
                    lista = lista.OrderByDescending(s => s.NombreUsuario).ToList();
                    break;
                case "Fecha_desc":
                    lista = lista.OrderByDescending(s => s.FechaEmision).ToList();
                    break;
                case "FechaFin_desc":
                    lista = lista.OrderByDescending(s => s.FechaConclusion).ToList();
                    break;
                case "Estado_desc":
                    lista = lista.OrderByDescending(s => s.Ejecutado).ToList();
                    break;
                default:
                    lista = lista.OrderByDescending(s => s.FechaEmision).ToList();
                    break;
            }

            int pageSize = 50;
            int pageNumber = (page ?? 1);

            return lista.ToPagedList(pageNumber, pageSize);
        }
        public ActionResult Comprobante(int? id = null, int? idTipoComprobante = null)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            ViewBag.Title += " - Comprobante";
            MenuNavBarSelected(2);
            UsuarioDTO currentUser = getCurrentUser();

            ComprobanteBL objBL = new ComprobanteBL();
            ViewBag.lstTipoDocumento = objBL.getTipoDeDocumentos();
            ViewBag.lstClientes = objBL.getListaClientesEnEmpresa(currentUser.IdEmpresa);
            ViewBag.lstProveedores = objBL.getListaProveedoresEnEmpresa(currentUser.IdEmpresa);
            ViewBag.lstMonedas = objBL.getListaMonedas();
            ViewBag.lstAreas = objBL.getListaAreasEnEmpresa(currentUser.IdEmpresa, true);
            ViewBag.lstResponsables = objBL.getListaResponsablesEnEmpresa(currentUser.IdEmpresa);
            ViewBag.lstHonorarios = objBL.getListaHonorariosEnEmpresa(currentUser.IdEmpresa);
            ViewBag.Proyectos = new List<ProyectoDTO>();
            ViewBag.Categorias = CategoriasBucle(null, null);

            var objSent = TempData["Comprobante"];
            if (objSent != null) { TempData["Comprobante"] = null; return View(objSent); }

            ComprobanteDTO obj;
            if (id != null && id != 0)
            {
                obj = objBL.getComprobanteEnEmpresa((int)currentUser.IdEmpresa, (int)id);
                if (obj == null) return RedirectToAction("Comprobantes");
                if (obj.IdEmpresa != currentUser.IdEmpresa) return RedirectToAction("Comprobantes");
                obj.UsuarioCreacion = currentUser.IdUsuario;
                ViewBag.Montos = obj.lstMontos;

                return View(obj);
            }
            obj = new ComprobanteDTO();
            obj.IdEmpresa = currentUser.IdEmpresa;
            obj.TipoCambio = (new EmpresaBL()).getEmpresa(currentUser.IdEmpresa).TipoCambio;
            obj.UsuarioCreacion = currentUser.IdUsuario;
            obj.FechaEmision = DateTime.Now;

            if (idTipoComprobante != null && idTipoComprobante != 0) obj.IdTipoComprobante = idTipoComprobante.GetValueOrDefault();
            return View(obj);
        }
        [HttpPost]
        public ActionResult AddComprobante(ComprobanteDTO dto)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            if (getCurrentUser().IdRol == 4) { return RedirectToAction("Comprobantes", "Admin"); }
            try
            {
                int TipoComprobante = 1; //Por defecto tipo de comprobante Ingreso
                if (dto != null)
                {
                    TipoComprobante = dto.IdTipoComprobante;
                    dto.lstMontos = (List<AreaPorComprobanteDTO>)TempData["AreasXMontos"] ?? new List<AreaPorComprobanteDTO>();
                }

                ComprobanteBL objBL = new ComprobanteBL();
                if (dto.IdComprobante == 0)
                {
                    if (objBL.add(dto))
                    {
                        createResponseMessage(CONSTANTES.SUCCESS);
                        return RedirectToAction("Comprobantes", "Admin", new { idTipoComprobante = TipoComprobante });
                    }
                }
                else if (dto.IdComprobante != 0)
                {
                    if (objBL.update(dto))
                    {
                        createResponseMessage(CONSTANTES.SUCCESS);
                        return RedirectToAction("Comprobantes", "Admin", new { idTipoComprobante = TipoComprobante });
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
                if (dto.IdComprobante != 0)
                    createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_UPDATE_MESSAGE);
                else createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_INSERT_MESSAGE);
            }
            TempData["Comprobante"] = dto;
            return RedirectToAction("Comprobante");
        }
        [HttpPost]
        public ActionResult DeleteComprobante(int id)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            if (getCurrentUser().IdRol == 4) { return RedirectToAction("Comprobantes", "Admin"); }

            try
            {
                ComprobanteBL objBL = new ComprobanteBL();
                if (objBL.delete(id))
                {
                    createResponseMessage(CONSTANTES.SUCCESS, CONSTANTES.SUCCESS_DELETE);
                }
                else
                {
                    createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_DELETE);
                }
            }
            catch (Exception e)
            {
                createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_NO_DELETE);
                throw;
            }
            return RedirectToAction("Comprobantes", "Admin");
        }
        public ActionResult Areas()
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            if (!isAdministrator()) { return RedirectToAction("Index"); }
            ViewBag.Title += " - Areas";
            MenuNavBarSelected(4, 1);
            UsuarioDTO currentUser = getCurrentUser();

            AreaBL objBL = new AreaBL();
            List<AreaDTO> listaAreas = new List<AreaDTO>();

            if (currentUser.IdEmpresa > 0)
            {
                listaAreas = objBL.getAreasEnEmpresa(currentUser.IdEmpresa);
            }
            return View(listaAreas);
        }
        public ActionResult Area(int? id = null)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            if (!this.isAdministrator()) { return RedirectToAction("Index"); }
            ViewBag.Title += " - Area";
            MenuNavBarSelected(4, 1);

            UsuarioDTO currentUser = getCurrentUser();

            AreaBL objBL = new AreaBL();

            var objSent = TempData["Area"];
            if (objSent != null) { TempData["Area"] = null; return View(objSent); }

            AreaDTO obj;
            if (id != null)
            {
                obj = objBL.getAreaEnEmpresa((int)currentUser.IdEmpresa, (int)id);
                if (obj == null) return RedirectToAction("Areas");
                if (obj.IdEmpresa != currentUser.IdEmpresa) return RedirectToAction("Areas");
                return View(obj);
            }
            obj = new AreaDTO();
            obj.IdEmpresa = currentUser.IdEmpresa;

            return View(obj);
        }
        [HttpPost]
        public ActionResult AddArea(AreaDTO dto)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            if (!this.isAdministrator()) { return RedirectToAction("Index"); }
            try
            {
                AreaBL objBL = new AreaBL();
                if (dto.IdArea == 0)
                {
                    if (objBL.add(dto))
                    {
                        createResponseMessage(CONSTANTES.SUCCESS);
                        return RedirectToAction("Areas");
                    }
                }
                else if (dto.IdArea != 0)
                {
                    if (objBL.update(dto))
                    {
                        createResponseMessage(CONSTANTES.SUCCESS);
                        return RedirectToAction("Areas");
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
                if (dto.IdArea != 0)
                    createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_UPDATE_MESSAGE);
                else createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_INSERT_MESSAGE);
            }
            TempData["Area"] = dto;
            return RedirectToAction("Area");
        }
        public ActionResult Responsables()
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            if (!isAdministrator()) { return RedirectToAction("Index"); }
            ViewBag.Title += " - Responsables";
            MenuNavBarSelected(4, 2);

            UsuarioDTO currentUser = getCurrentUser();

            ResponsableBL objBL = new ResponsableBL();
            List<ResponsableDTO> listaResponsables = new List<ResponsableDTO>();

            if (currentUser.IdEmpresa > 0)
            {
                listaResponsables = objBL.getResponsablesEnEmpresa(currentUser.IdEmpresa);
            }
            return View(listaResponsables);
        }
        public ActionResult Responsable(int? id = null)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            if (!this.isAdministrator()) { return RedirectToAction("Index"); }
            ViewBag.Title += " - Responsable";
            MenuNavBarSelected(4, 2);

            UsuarioDTO currentUser = getCurrentUser();

            ResponsableBL objBL = new ResponsableBL();

            var objSent = TempData["Responsable"];
            if (objSent != null) { TempData["Responsable"] = null; return View(objSent); }

            ResponsableDTO obj;
            if (id != null)
            {
                obj = objBL.getResponsableEnEmpresa((int)currentUser.IdEmpresa, (int)id);
                if (obj == null) return RedirectToAction("Responsables");
                if (obj.IdEmpresa != currentUser.IdEmpresa) return RedirectToAction("Responsables");
                return View(obj);
            }
            obj = new ResponsableDTO();
            obj.IdEmpresa = currentUser.IdEmpresa;

            return View(obj);
        }
        [HttpPost]
        public ActionResult AddResponsable(ResponsableDTO dto)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            try
            {
                ResponsableBL objBL = new ResponsableBL();
                if (dto.IdResponsable == 0)
                {
                    if (objBL.add(dto))
                    {
                        createResponseMessage(CONSTANTES.SUCCESS);
                        return RedirectToAction("Responsables");
                    }
                }
                else if (dto.IdResponsable != 0)
                {
                    if (objBL.update(dto))
                    {
                        createResponseMessage(CONSTANTES.SUCCESS);
                        return RedirectToAction("Responsables");
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
                if (dto.IdResponsable != 0)
                    createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_UPDATE_MESSAGE);
                else createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_INSERT_MESSAGE);
            }
            TempData["Responsable"] = dto;
            return RedirectToAction("Responsable");
        }

        public ActionResult Honorarios()
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            if (!isAdministrator()) { return RedirectToAction("Index"); }
            ViewBag.Title += " - Honorarios";
            MenuNavBarSelected(4, 4);

            UsuarioDTO currentUser = getCurrentUser();

            HonorarioBL objBL = new HonorarioBL();
            List<HonorarioDTO> listaHonorarios = new List<HonorarioDTO>();

            if (currentUser.IdEmpresa > 0)
            {
                listaHonorarios = objBL.getHonorariosEnEmpresa(currentUser.IdEmpresa);
            }
            return View(listaHonorarios);
        }

        public ActionResult Honorario(int? id = null)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            if (!this.isAdministrator()) { return RedirectToAction("Index"); }
            ViewBag.Title += " - Honorario";
            MenuNavBarSelected(4, 4);

            UsuarioDTO currentUser = getCurrentUser();

            HonorarioBL objBL = new HonorarioBL();

            var objSent = TempData["Honorario"];
            if (objSent != null) { TempData["Honorario"] = null; return View(objSent); }

            HonorarioDTO obj;
            if (id != null)
            {
                obj = objBL.getHonorarioEnEmpresa((int)currentUser.IdEmpresa, (int)id);
                if (obj == null) return RedirectToAction("Honorarios");
                if (obj.IdEmpresa != currentUser.IdEmpresa) return RedirectToAction("Honorarios");
                return View(obj);
            }
            obj = new HonorarioDTO();
            obj.IdEmpresa = currentUser.IdEmpresa;

            return View(obj);
        }
        [HttpPost]
        public ActionResult AddHonorario(HonorarioDTO dto)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            try
            {
                HonorarioBL objBL = new HonorarioBL();
                if (dto.IdHonorario == 0)
                {
                    if (objBL.add(dto))
                    {
                        createResponseMessage(CONSTANTES.SUCCESS);
                        return RedirectToAction("Honorarios");
                    }
                }
                else if (dto.IdHonorario != 0)
                {
                    if (objBL.update(dto))
                    {
                        createResponseMessage(CONSTANTES.SUCCESS);
                        return RedirectToAction("Honorarios");
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
                if (dto.IdHonorario != 0)
                    createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_UPDATE_MESSAGE);
                else createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_INSERT_MESSAGE);
            }
            TempData["Honorario"] = dto;
            return RedirectToAction("Honorario");
        }

        public ActionResult Proyectos()
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            ProyectoBL objBL = new ProyectoBL();
            return View(objBL.getProyectos());
        }
        public ActionResult Proyecto(int? id = null, int? idEntidad = null)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            //if (!this.isAdministrator()) { return RedirectToAction("Index"); }
            ViewBag.Title += " - Proyecto";
            MenuNavBarSelected(4, 4);
            UsuarioDTO miUsuario = getCurrentUser();

            ProyectoBL objBL = new ProyectoBL();
            ViewBag.IdProyecto = id;

            EntidadResponsableBL objEntidadBL = new EntidadResponsableBL();
            EntidadResponsableDTO objEntidad = objEntidadBL.getEntidadResponsableEnEmpresa(miUsuario.IdEmpresa, idEntidad.GetValueOrDefault());
            if (objEntidad == null) { return RedirectToAction("Entidades", "Admin"); }

            var objSent = TempData["Proyecto"];
            if (objSent != null) { TempData["Proyecto"] = null; return View(objSent); }
            if (id == 0 && idEntidad != null)
            {
                ProyectoDTO nuevo = new ProyectoDTO();
                nuevo.IdEntidadResponsable = (int)idEntidad;
                nuevo.Estado = true;
                return View(nuevo);
            }
            else
            {
                if (id != null)
                {
                    ProyectoDTO obj = objBL.getProyecto((int)id);

                    if (obj == null) return RedirectToAction("Entidad", "Admin", new { id = objEntidad.IdEntidadResponsable });
                    if (obj.IdEntidadResponsable != objEntidad.IdEntidadResponsable) return RedirectToAction("Entidad", "Admin", new { id = objEntidad.IdEntidadResponsable });

                    EntidadResponsableDTO objEntidadProy = objEntidadBL.getEntidadResponsableEnEmpresa(miUsuario.IdEmpresa, obj.IdEntidadResponsable);
                    if (objEntidadProy == null) return RedirectToAction("Entidades", "Admin");
                    if (objEntidadProy.IdEmpresa != miUsuario.IdEmpresa) return RedirectToAction("Entidades", "Admin");

                    return View(obj);
                }
            }
            return View();
        }
        public ActionResult AddProyecto(ProyectoDTO dto)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            try
            {
                ProyectoBL objBL = new ProyectoBL();
                if (dto.IdProyecto == 0)
                {
                    if (objBL.add(dto))
                    {
                        //objBL.ActualizarSaldos(dto.IdCuentaBancaria);
                        createResponseMessage(CONSTANTES.SUCCESS);
                        return RedirectToAction("Entidad", new { id = dto.IdEntidadResponsable });
                    }
                }
                else if (dto.IdProyecto != 0)
                {
                    if (objBL.update(dto))
                    {
                        //objBL.ActualizarSaldos(dto.IdCuentaBancaria);
                        createResponseMessage(CONSTANTES.SUCCESS);
                        return RedirectToAction("Entidad", new { id = dto.IdEntidadResponsable });
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
                if (dto.IdProyecto != 0)
                    createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_UPDATE_MESSAGE);
                else createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_INSERT_MESSAGE);
            }
            TempData["Proyecto"] = dto;
            return RedirectToAction("Proyecto");
        }
        public ActionResult Contactos()
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            ContactoBL objBL = new ContactoBL();
            return View(objBL.getContactos());
        }
        public ActionResult Contacto(int? id = null, int? idEntidad = null)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            //if (!this.isAdministrator()) { return RedirectToAction("Index"); }
            ViewBag.Title += " - Contacto";
            MenuNavBarSelected(4, 4);
            UsuarioDTO miUsuario = getCurrentUser();

            ContactoBL objBL = new ContactoBL();
            ViewBag.IdContacto = id;

            EntidadResponsableBL objEntidadBL = new EntidadResponsableBL();
            EntidadResponsableDTO objEntidad = objEntidadBL.getEntidadResponsableEnEmpresa(miUsuario.IdEmpresa, idEntidad.GetValueOrDefault());
            if (objEntidad == null) { return RedirectToAction("Entidades", "Admin"); }

            var objSent = TempData["Contacto"];
            if (objSent != null) { TempData["Contacto"] = null; return View(objSent); }
            if (id == 0 && idEntidad != null)
            {
                ContactoDTO nuevo = new ContactoDTO();
                nuevo.IdEntidadResponsable = (int)idEntidad;
                nuevo.Estado = true;
                return View(nuevo);
            }
            else
            {
                if (id != null)
                {
                    ContactoDTO obj = objBL.getContacto((int)id);

                    if (obj == null) return RedirectToAction("Entidad", "Admin", new { id = objEntidad.IdEntidadResponsable });
                    if (obj.IdEntidadResponsable != objEntidad.IdEntidadResponsable) return RedirectToAction("Entidad", "Admin", new { id = objEntidad.IdEntidadResponsable });

                    EntidadResponsableDTO objEntidadProy = objEntidadBL.getEntidadResponsableEnEmpresa(miUsuario.IdEmpresa, obj.IdEntidadResponsable);
                    if (objEntidadProy == null) return RedirectToAction("Entidades", "Admin");
                    if (objEntidadProy.IdEmpresa != miUsuario.IdEmpresa) return RedirectToAction("Entidades", "Admin");

                    return View(obj);
                }
            }
            return View();
        }
        public ActionResult AddContacto(ContactoDTO dto)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            try
            {
                ContactoBL objBL = new ContactoBL();
                if (dto.IdContacto == 0)
                {
                    if (objBL.add(dto))
                    {
                        //objBL.ActualizarSaldos(dto.IdCuentaBancaria);
                        createResponseMessage(CONSTANTES.SUCCESS);
                        return RedirectToAction("Entidad", new { id = dto.IdEntidadResponsable });
                    }
                }
                else if (dto.IdContacto != 0)
                {
                    if (objBL.update(dto))
                    {
                        //objBL.ActualizarSaldos(dto.IdCuentaBancaria);
                        createResponseMessage(CONSTANTES.SUCCESS);
                        return RedirectToAction("Entidad", new { id = dto.IdEntidadResponsable });
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
                if (dto.IdContacto != 0)
                    createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_UPDATE_MESSAGE);
                else createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_INSERT_MESSAGE);
            }
            TempData["Contacto"] = dto;
            return RedirectToAction("Contacto");
        }

        public ActionResult Periodos()
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            if (!isAdministrator()) { return RedirectToAction("Index"); }
            ViewBag.Title += " - Periodos";
            MenuNavBarSelected(4, 6);

            UsuarioDTO currentUser = getCurrentUser();

            PeriodoBL objBL = new PeriodoBL();
            List<PeriodoDTO> listaPeriodos = new List<PeriodoDTO>();

            if (currentUser.IdEmpresa > 0)
            {
                listaPeriodos = objBL.getPeriodosEnEmpresa(currentUser.IdEmpresa);
            }
            return View(listaPeriodos);
        }

        public ActionResult Periodo(int? id = null)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            if (!this.isAdministrator()) { return RedirectToAction("Index"); }
            ViewBag.Title += " - Periodo";
            MenuNavBarSelected(4, 6);

            UsuarioDTO currentUser = getCurrentUser();

            PeriodoBL objBL = new PeriodoBL();

            var objSent = TempData["Periodo"];
            if (objSent != null) { TempData["Periodo"] = null; return View(objSent); }

            PeriodoDTO obj;
            if (id != null)
            {
                obj = objBL.getPeriodoEnEmpresa((int)currentUser.IdEmpresa, (int)id);
                if (obj == null) return RedirectToAction("Periodos");
                if (obj.IdEmpresa != currentUser.IdEmpresa) return RedirectToAction("Periodos");
                return View(obj);
            }
            obj = new PeriodoDTO();
            obj.IdEmpresa = currentUser.IdEmpresa;

            int dyear = DateTime.Now.Year;
            obj.FechaInicio = new DateTime(dyear, 1, 1);
            obj.FechaFin = new DateTime(dyear, 12, 31);
            return View(obj);
        }

        [HttpPost]
        public ActionResult AddPeriodo(PeriodoDTO dto)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            try
            {
                PeriodoBL objBL = new PeriodoBL();
                if (dto.IdPeriodo == 0)
                {
                    if (objBL.add(dto))
                    {
                        createResponseMessage(CONSTANTES.SUCCESS);
                        return RedirectToAction("Periodos");
                    }
                }
                else if (dto.IdPeriodo != 0)
                {
                    if (objBL.update(dto))
                    {
                        createResponseMessage(CONSTANTES.SUCCESS);
                        return RedirectToAction("Periodos");
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
                if (dto.IdPeriodo != 0)
                    createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_UPDATE_MESSAGE);
                else createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_INSERT_MESSAGE);
            }
            TempData["Periodo"] = dto;
            return RedirectToAction("Periodo");
        }

        #region APIs adicionales
        public JsonResult CategoriasJson()
        {
            //List<Select2DTO> ListaCategorias = new List<Select2DTO>();
            CategoriaBL objBL = new CategoriaBL();
            var listaCat = CategoriasBucle(null, null);

            return Json(new { listaCat }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetPeriodos()
        {
            PeriodoBL periodoBL = new PeriodoBL();
            var periodos = periodoBL.getPeriodosEnEmpresa(getCurrentUser().IdEmpresa);
            return Json(periodos, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GetComprobantes(int idEntidad, int idTipoDoc)
        {
            ComprobanteBL objBL = new ComprobanteBL();
            var listaComp = objBL.getComprobantesPorEntXTDoc(getCurrentUser().IdEmpresa, idEntidad, idTipoDoc);
            return Json(new { listaComp }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GetProyectos(int idEntidad)
        {
            ProyectoBL objBL = new ProyectoBL();
            var listaProyectos = objBL.getProyectosPorEntidad(idEntidad, true);
            return Json(new { listaProyectos }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public string ActualizarEstadoEnMovimiento(int idMovimiento)
        {
            if (!this.currentUser() || isUsuarioExterno()) return "false";

            UsuarioDTO miUsuario = getCurrentUser();
            MovimientoBL obj = new MovimientoBL();
            obj.ActualizarEstadoMovimiento(idMovimiento);
            return "true";
        }

        [HttpPost]
        public string ActualizarPeriodo(int idPeriodo)
        {
            if (!this.currentUser() || !isAdministrator()) { return "false"; }

            EmpresaBL objBL = new EmpresaBL();
            UsuarioDTO miUsuario = getCurrentUser();
            EmpresaDTO obj = new EmpresaDTO() { IdEmpresa = miUsuario.IdEmpresa, IdPeriodo = idPeriodo };
            if (objBL.updatePeriodo(obj))
            {
                return "true";
            }
            return "false";
        }

        [HttpPost]
        public JsonResult ActualizarPresupuesto(int idCategoria, Decimal Monto)
        {
            if (!this.currentUser() || !isAdministrator()) { return Json(false, JsonRequestBehavior.AllowGet); }

            CategoriaBL objBL = new CategoriaBL();
            EmpresaBL empBL = new EmpresaBL();
            int pPeriodo = empBL.getEmpresa(getCurrentUser().IdEmpresa).IdPeriodo.GetValueOrDefault();
            if (pPeriodo == 0) { return Json(false, JsonRequestBehavior.AllowGet); }

            CategoriaPorPeriodoDTO dto = new CategoriaPorPeriodoDTO() { IdCategoria = idCategoria, IdPeriodo = pPeriodo, Monto = Monto };
            objBL.updatePresupuesto(dto);

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarComprobante(int idComprobante, int idCuentaBancaria)
        {
            ComprobanteBL objBL = new ComprobanteBL();
            var comprobante = objBL.getComprobanteEjecutadoEnEmpresa(idComprobante, idCuentaBancaria, getCurrentUser().IdEmpresa);
            return Json(new { comprobante }, JsonRequestBehavior.AllowGet);
        }

        public List<Select2DTO> CategoriasBucle(int? id = null, IList<CategoriaDTO> lista = null)
        {
            var listaCat = lista;
            if (id == null && lista == null)
            {
                CategoriaBL objBL = new CategoriaBL();
                listaCat = objBL.getCategoriasTreeEnEmpresa(getCurrentUser().IdEmpresa);
            }
            List<Select2DTO> selectTree = new List<Select2DTO>();

            foreach (var item in listaCat)
            {
                if (item.Estado)
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
        public ActionResult ReporteCategorias(int? message = null)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            //No hay reportes para usuarios internos
            if (getCurrentUser().IdRol == 3) { return RedirectToAction("Ingresar"); }

            MenuNavBarSelected(3);

            CuentaBancariaBL objBL = new CuentaBancariaBL();
            ViewBag.Categorias = CategoriasBucle(null, null);
            //ViewBag.Libros = objBL.getCuentasBancariasEnEmpresaBag((int)getCurrentUser().IdEmpresa, true);

            if (message != null)
            {
                switch (message)
                {
                    case 1:
                        createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_MESSAGE);
                        break;
                    case 2:
                        createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_REPORTE_NO_MOVS);
                        break;
                }
            }

            return View();
        }

        /*
        [HttpGet]
        public ActionResult GenerarReporteCategorias(int? IdCuentaB, DateTime? FechaInicio, DateTime? FechaFin)
        {
            if (IdCuentaB == null || FechaInicio == null || FechaFin == null)
            {

                return RedirectToAction("ReporteCategorias", new { message = 1 });
            }

            CategoriaBL objBL = new CategoriaBL();
            var data = objBL.getReporteCategorias(IdCuentaB, FechaInicio, FechaFin);

            //Lista de Categorias
            List<CategoriaDTO> lstCats = objBL.getCategorias();

            if (data == null)
                return RedirectToAction("ReporteCategorias", new { message = 2 });

            System.Data.DataTable dt = new System.Data.DataTable();
            dt.Clear();


            //dt.Columns.Add("Categorias");

            //Llenado de Padres
            for (int i = 0; i < data.Count; i++)
            {
                data[i] = objBL.obtenerPadreEntidadReporte(data[i], lstCats, 0);
            }

            //Columnas de Categorias
            for (int i = 0; i <= CONSTANTES.NivelCat; i++)
            {
                //dt.Columns.Add("Categoria N." + i.ToString());
                switch (i)
                {
                    case 0:
                        dt.Columns.Add("Categoria");
                        break;
                    default:
                        dt.Columns.Add("Categoria Sub " + i.ToString());
                        break;
                }
            }
            dt.Columns.Add("Montos Totales");

            //Ultimo Data Row
            System.Data.DataRow lastRow = dt.NewRow();
            Decimal MontoCategoria = 0;
            Decimal MontoSubCategoria = 0;
            //bool inicio = false;

            //Pintado de Padres
            for (int i = 0; i < data.Count; i++)
            {
                System.Data.DataRow row = dt.NewRow();

                row = DameRowPintarPadres(row, data[i]);
                row["Montos Totales"] = data[i].MontoTotal.ToString("N2", CultureInfo.InvariantCulture);
                dt.Rows.Add(row);

                if (i + 1 < data.Count)
                {
                    System.Data.DataRow rowFutura = dt.NewRow();
                    rowFutura = DameRowPintarPadres(rowFutura, data[i + 1]);

                    string miCadena = (string)row["Categoria"];
                    string miCadena2 = (string)rowFutura["Categoria"];
                    //if (miCadena != "OTROS" && miCadena != "INGRESOS")
                    //{
                        MontoCategoria += data[i].MontoTotal;
                        MontoSubCategoria += data[i].MontoTotal;
                        if (CONSTANTES.NivelCat > 0)
                        {
                            if (row["Categoria Sub 1"] != rowFutura["Categoria Sub 1"] && !string.IsNullOrEmpty(row["Categoria Sub 1"].ToString()))
                            {
                                System.Data.DataRow aux1 = dt.NewRow();
                                aux1["Categoria Sub 1"] = "TOTAL :";
                                aux1["Montos Totales"] = MontoSubCategoria.ToString("N2", CultureInfo.InvariantCulture);
                                dt.Rows.Add(aux1);
                                MontoSubCategoria = 0;
                            }
                        }
                        if (row["Categoria"] != rowFutura["Categoria"])
                        {
                            MontoSubCategoria = 0;
                            System.Data.DataRow aux = dt.NewRow();
                            aux["Categoria"] = "TOTAL :";
                            aux["Montos Totales"] = MontoCategoria.ToString("N2", CultureInfo.InvariantCulture);
                            dt.Rows.Add(aux);
                            MontoCategoria = 0;
                        }
                    //}
                }
                else
                {
                    MontoCategoria += data[i].MontoTotal;
                    MontoSubCategoria += data[i].MontoTotal;
                    //Sub Categoria
                    if(CONSTANTES.NivelCat > 0)
                    { 
                        System.Data.DataRow aux1 = dt.NewRow();
                        aux1["Categoria Sub 1"] = "TOTAL :";
                        aux1["Montos Totales"] = MontoSubCategoria.ToString("N2", CultureInfo.InvariantCulture);
                        dt.Rows.Add(aux1);
                        MontoSubCategoria = 0;
                    }
                    //Categoria Principal
                    System.Data.DataRow aux = dt.NewRow();
                    aux["Categoria"] = "TOTAL :";
                    aux["Montos Totales"] = MontoCategoria.ToString("N2", CultureInfo.InvariantCulture);
                    dt.Rows.Add(aux);
                    MontoCategoria = 0;
                }
            }

            GridView gv = new GridView();

            gv.DataSource = dt;
            gv.AllowPaging = false;
            gv.DataBind();

            if (dt.Rows.Count > 0)
            {
                CuentaBancariaBL oBL = new CuentaBancariaBL();
                CuentaBancariaDTO obj = oBL.getCuentaBancaria(IdCuentaB.GetValueOrDefault());

                PintarCabeceraTabla(gv);
                PintarIntercaladoCategorias(gv);

                AddSuperHeader(gv, "RESUMEN DE MOVIMIENTOS EN CATEGORIAS - Libro:" + obj.NombreCuenta);
                //Cabecera principal
                AddWhiteHeader(gv, 1, "");
                AddWhiteHeader(gv, 2, "Periodo del reporte: " + FechaInicio.GetValueOrDefault().ToShortDateString() + " - " + FechaFin.GetValueOrDefault().ToShortDateString());
                AddWhiteHeader(gv, 3, "Fecha de conciliaci&oacute;n actual: " + obj.FechaConciliacion.ToShortDateString());
                AddWhiteHeader(gv, 4, "Moneda: " + obj.NombreMoneda);

                PintarCategorias(gv);

                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=" + obj.NombreCuenta + "_" + DateTime.Now.ToString("dd-MM-yyyy") + "_RCategorias.xls");
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
            return RedirectToAction("ReporteCategorias", new { message = 2 });
            //return View();
        }
        public ActionResult GenerarReporteDetalleMovimientos(int? IdCuentaB, DateTime? FechaInicio, DateTime? FechaFin)
        {
            if (IdCuentaB == null || FechaInicio == null || FechaFin == null)
            {

                return RedirectToAction("ReporteCategorias", new { message = 1 });
            }

            MovimientoBL objBL = new MovimientoBL();

            var data = objBL.getReporteDetalleLibro(IdCuentaB, FechaInicio, FechaFin);

            if (data == null)
                return RedirectToAction("ReporteCategorias", new { message = 2 });

            System.Data.DataTable dt = new System.Data.DataTable();
            dt.Clear();

            dt.Columns.Add("Nombre");
            dt.Columns.Add("Categoria");
            dt.Columns.Add("Entidad");
            dt.Columns.Add("Fecha");
            dt.Columns.Add("N° Doc");
            dt.Columns.Add("Monto");
            dt.Columns.Add("Tipo");
            dt.Columns.Add("Estado");
            dt.Columns.Add("Comentario");

            foreach (var item in data)
            {
                System.Data.DataRow row = dt.NewRow();

                row["Nombre"] = item.NroOperacion;
                row["Categoria"] = item.NombreCategoria;
                row["Entidad"] = item.NombreEntidadR;
                row["Fecha"] = item.Fecha.ToShortDateString();
                row["N° Doc"] = (item.NumeroDocumento != null && item.NumeroDocumento != "0") ? item.NumeroDocumento : "N/A";
                row["Monto"] = item.Monto.ToString("N2", CultureInfo.InvariantCulture);
                row["Tipo"] = item.IdTipoMovimiento == 1 ? "Entrada" : "Salida";
                row["Estado"] = item.IdEstadoMovimiento == 1 ? "Pendiente" : "Realizado";
                row["Comentario"] = item.Comentario == "No existe comentario" ? "" : item.Comentario;

                dt.Rows.Add(row);
            }

            GridView gv = new GridView();

            gv.DataSource = dt;
            gv.AllowPaging = false;
            gv.DataBind();

            if (dt.Rows.Count > 0)
            {
                CuentaBancariaBL oBL = new CuentaBancariaBL();
                CuentaBancariaDTO obj = oBL.getCuentaBancaria(IdCuentaB.GetValueOrDefault());

                PintarCabeceraTabla(gv);
                PintarColumnaNegrita(gv, 5, true);

                AddSuperHeader(gv, "DETALLE DE MOVIMIENTOS - Libro:" + obj.NombreCuenta);
                //Cabecera principal
                AddWhiteHeader(gv, 1, "");
                AddWhiteHeader(gv, 2, "Periodo del reporte: " + FechaInicio.GetValueOrDefault().ToShortDateString() + " - " + FechaFin.GetValueOrDefault().ToShortDateString());
                AddWhiteHeader(gv, 3, "Fecha de conciliaci&oacute;n actual: " + obj.FechaConciliacion.ToShortDateString());
                AddWhiteHeader(gv, 4, "Moneda: " + obj.NombreMoneda);

                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=" + obj.NombreCuenta + "_" + DateTime.Now.ToString("dd-MM-yyyy") + "DMovimientos.xls");
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
            return RedirectToAction("ReporteCategorias", new { message = 2 });
        }
        public ActionResult GenerarReporteResumenEntidadesR(int? IdCuentaB, DateTime? FechaInicio, DateTime? FechaFin)
        {
            if (IdCuentaB == null || FechaInicio == null || FechaFin == null)
            {
                return RedirectToAction("ReporteCategorias", new { message = 1 });
            }

            EntidadResponsableBL objBL = new EntidadResponsableBL();

            var data = objBL.getReporteResumenEntidadesR(IdCuentaB, FechaInicio, FechaFin);

            if (data == null)
                return RedirectToAction("ReporteCategorias", new { message = 2 });

            System.Data.DataTable dt = new System.Data.DataTable();
            dt.Clear();

            dt.Columns.Add("Entidad Responsable");
            dt.Columns.Add("Tipo de Bien o Servicio");
            dt.Columns.Add("Detracción");
            dt.Columns.Add("Montos Totales");


            foreach (var item in data)
            {
                System.Data.DataRow row = dt.NewRow();

                row["Entidad Responsable"] = item.Nombre;
                row["Tipo de Bien o Servicio"] = item.Tipo;
                row["Detracción"] = item.Detraccion;
                row["Montos Totales"] = item.Monto.ToString("N2", CultureInfo.InvariantCulture); ;

                dt.Rows.Add(row);
            }

            GridView gv = new GridView();

            gv.DataSource = dt;
            gv.AllowPaging = false;
            gv.DataBind();

            if (dt.Rows.Count > 0)
            {
                CuentaBancariaBL oBL = new CuentaBancariaBL();
                CuentaBancariaDTO obj = oBL.getCuentaBancaria(IdCuentaB.GetValueOrDefault());

                PintarCabeceraTabla(gv);
                PintarColumnaNegrita(gv, 3, true);

                AddSuperHeader(gv, "RESUMEN DE MOVIMIENTOS EN ENTIDADES - Libro:" + obj.NombreCuenta);
                //Cabecera principal
                AddWhiteHeader(gv, 1, "");
                AddWhiteHeader(gv, 2, "Periodo del reporte: " + FechaInicio.GetValueOrDefault().ToShortDateString() + " - " + FechaFin.GetValueOrDefault().ToShortDateString());
                AddWhiteHeader(gv, 3, "Fecha de conciliaci&oacute;n actual: " + obj.FechaConciliacion.ToShortDateString());
                AddWhiteHeader(gv, 4, "Moneda: " + obj.NombreMoneda);

                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=" + obj.NombreCuenta + "_" + DateTime.Now.ToString("dd-MM-yyyy") + "REntidades.xls");
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
            return RedirectToAction("ReporteCategorias", new { message = 2 });
        }
         */

        #region Reportes
        public ActionResult GenerarRep_AvanceDePresupuesto(DateTime? FechaInicio, DateTime? FechaFin)
        {
            if (FechaInicio == null || FechaFin == null)
            {
                return RedirectToAction("ReporteCategorias", new { message = 1 });
            }

            EmpresaDTO objEmpresa = (new EmpresaBL()).getEmpresa(getCurrentUser().IdEmpresa);

            ReportesBL repBL = new ReportesBL();
            List<CategoriaDTO> lstCatsMontos = repBL.AvanceDePresupuesto(objEmpresa.IdEmpresa, FechaInicio, FechaFin);

            //Sumatoria de Presupuestos de Padres y armado de Arbol
            List<CategoriaDTO> lstCats = repBL.getCategoriasTreeEnEmpresa(lstCatsMontos, objEmpresa.IdEmpresa);
            //Arbol de presupuestos
            CategoriaBL catBL = new CategoriaBL();
            List<CategoriaDTO> arbolPresupuestos = repBL.getCategoriasPresupuestosTreeEnEmpresa(objEmpresa.IdEmpresa, 0);

            if (lstCats == null)
                return RedirectToAction("ReporteCategorias", new { message = 2 });

            System.Data.DataTable dt = new System.Data.DataTable();
            dt.Clear();

            dt.Columns.Add("Nivel");
            dt.Columns.Add("P. Presupuesto");
            dt.Columns.Add("Total (" + objEmpresa.SimboloMoneda + ")");
            dt.Columns.Add("PRESUPUESTO ANUAL (" + objEmpresa.SimboloMoneda + ")");
            dt.Columns.Add("PRESUPUESTO EJECUTADO A LA FECHA %");

            //Suma de Padres de Nivel 0
            Decimal SumaPadres0 = lstCatsMontos.Where(x => x.IdCategoriaPadre == null).Sum(x => x.Presupuesto.GetValueOrDefault());
            Decimal SumaPresupuesto = arbolPresupuestos.Sum(x => x.Presupuesto.GetValueOrDefault());

            System.Data.DataRow auxRow = dt.NewRow();
            auxRow[0] = ""; auxRow[1] = ""; auxRow[2] = SumaPadres0.ToString("N2", CultureInfo.InvariantCulture); auxRow[3] = SumaPresupuesto.ToString("N2", CultureInfo.InvariantCulture); auxRow[4] = (SumaPresupuesto == 0) ? "0.00%" : (SumaPresupuesto / SumaPresupuesto).ToString("P2", CultureInfo.InvariantCulture);
            dt.Rows.Add(auxRow);

            foreach (var obj in arbolPresupuestos)
            {
                PintarArbolPadre(obj, lstCatsMontos, objEmpresa, dt);
            }

            GridView gv = new GridView();

            gv.DataSource = dt;
            gv.AllowPaging = false;
            gv.DataBind();

            if (dt.Rows.Count > 0)
            {
                PintarCabeceraTabla(gv);
                //PintarIntercaladoCategorias(gv);

                AddSuperHeader(gv, "Avance de Presupuesto - Empresa:" + objEmpresa.Nombre);
                //Cabecera principal
                AddWhiteHeader(gv, 1, "");
                AddWhiteHeader(gv, 2, "PERIODO: " + FechaInicio.GetValueOrDefault().ToShortDateString() + " - " + FechaFin.GetValueOrDefault().ToShortDateString());
                AddWhiteHeader(gv, 3, "Moneda: (" + objEmpresa.SimboloMoneda + ")");

                PintarCategorias(gv);

                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=" + "AvanceDePresupuesto_" + objEmpresa.Nombre + "_" + DateTime.Now.ToString("dd-MM-yyyy") + ".xls");
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
            return RedirectToAction("ReporteCategorias", new { message = 2 });
        }
        public ActionResult GenerarRep_EgresosPorAreas(DateTime? FechaInicio, DateTime? FechaFin)
        {
            if (FechaInicio == null || FechaFin == null)
            {
                return RedirectToAction("ReporteCategorias", new { message = 1 });
            }

            EmpresaDTO objEmpresa = (new EmpresaBL()).getEmpresa(getCurrentUser().IdEmpresa);

            ReportesBL repBL = new ReportesBL();
            List<AreaDTO> lstAreasMontos = repBL.getEgresosAreasEnEmpresa(objEmpresa.IdEmpresa, FechaInicio, FechaFin);

            if (lstAreasMontos == null)
                return RedirectToAction("ReporteCategorias", new { message = 2 });

            System.Data.DataTable dt = new System.Data.DataTable();
            dt.Clear();

            dt.Columns.Add("Áreas");
            dt.Columns.Add("Clientes");
            dt.Columns.Add("Montos");
            dt.Columns.Add("Porcentaje");

            Decimal SumaTotal = lstAreasMontos.Sum(x => x.SumaClientes);

            foreach (var obj in lstAreasMontos)
            {
                PintarAreas(obj, SumaTotal, dt);
            }

            System.Data.DataRow rowFinal = dt.NewRow();
            rowFinal[0] = "TOTAL";
            rowFinal[2] = SumaTotal.ToString("N2", CultureInfo.InvariantCulture);
            dt.Rows.Add(rowFinal);

            GridView gv = new GridView();

            gv.DataSource = dt;
            gv.AllowPaging = false;
            gv.DataBind();

            if (dt.Rows.Count > 0)
            {
                PintarCabeceraTabla(gv);
                //PintarIntercaladoCategorias(gv);

                AddSuperHeader(gv, "Egresos por &aacute;reas - Empresa:" + objEmpresa.Nombre);
                //Cabecera principal
                AddWhiteHeader(gv, 1, "");
                AddWhiteHeader(gv, 2, "PERIODO: " + FechaInicio.GetValueOrDefault().ToShortDateString() + " - " + FechaFin.GetValueOrDefault().ToShortDateString());
                AddWhiteHeader(gv, 3, "Moneda: (" + objEmpresa.SimboloMoneda + ")");

                //PintarCategorias(gv);

                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=" + "EgresosPorAreas_" + objEmpresa.Nombre + "_" + DateTime.Now.ToString("dd-MM-yyyy") + ".xls");
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
            return RedirectToAction("ReporteCategorias", new { message = 2 });
        }
        public ActionResult GenerarRep_FacturacionPorAreas(DateTime? FechaInicio, DateTime? FechaFin)
        {
            if (FechaInicio == null || FechaFin == null)
            {
                return RedirectToAction("ReporteCategorias", new { message = 1 });
            }

            EmpresaDTO objEmpresa = (new EmpresaBL()).getEmpresa(getCurrentUser().IdEmpresa);

            ReportesBL repBL = new ReportesBL();
            List<AreaDTO> lstAreasMontos = repBL.getAreasEnEmpresa(objEmpresa.IdEmpresa, FechaInicio, FechaFin);

            if (lstAreasMontos == null)
                return RedirectToAction("ReporteCategorias", new { message = 2 });

            System.Data.DataTable dt = new System.Data.DataTable();
            dt.Clear();

            dt.Columns.Add("Áreas");
            dt.Columns.Add("Entidades");
            dt.Columns.Add("Montos");
            dt.Columns.Add("Porcentaje");

            Decimal SumaTotal = lstAreasMontos.Sum(x => x.SumaClientes);

            foreach (var obj in lstAreasMontos)
            {
                PintarAreas(obj, SumaTotal, dt);
            }

            System.Data.DataRow rowFinal = dt.NewRow();
            rowFinal[0] = "TOTAL";
            rowFinal[2] = SumaTotal.ToString("N2", CultureInfo.InvariantCulture);
            dt.Rows.Add(rowFinal);

            GridView gv = new GridView();

            gv.DataSource = dt;
            gv.AllowPaging = false;
            gv.DataBind();

            if (dt.Rows.Count > 0)
            {
                PintarCabeceraTabla(gv);
                //PintarIntercaladoCategorias(gv);

                AddSuperHeader(gv, "Ingresos por &aacute;reas - Empresa:" + objEmpresa.Nombre);
                //Cabecera principal
                AddWhiteHeader(gv, 1, "");
                AddWhiteHeader(gv, 2, "PERIODO: " + FechaInicio.GetValueOrDefault().ToShortDateString() + " - " + FechaFin.GetValueOrDefault().ToShortDateString());
                AddWhiteHeader(gv, 3, "Moneda: (" + objEmpresa.SimboloMoneda + ")");

                //PintarCategorias(gv);

                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=" + "IngresosPorAreas_" + objEmpresa.Nombre + "_" + DateTime.Now.ToString("dd-MM-yyyy") + ".xls");
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
            return RedirectToAction("ReporteCategorias", new { message = 2 });
        }
        public ActionResult GenerarRep_IngresosEgresosPorAreas(DateTime? FechaInicio, DateTime? FechaFin)
        {
            if (FechaInicio == null || FechaFin == null)
            {
                return RedirectToAction("ReporteCategorias", new { message = 1 });
            }

            EmpresaDTO objEmpresa = (new EmpresaBL()).getEmpresa(getCurrentUser().IdEmpresa);

            ReportesBL repBL = new ReportesBL();
            List<AreaDTO> lstAreasIE = repBL.getIngresosEgresosAreas(objEmpresa.IdEmpresa, FechaInicio, FechaFin);

            if (lstAreasIE == null)
                return RedirectToAction("ReporteCategorias", new { message = 2 });

            System.Data.DataTable dt = new System.Data.DataTable();
            dt.Clear();

            dt.Columns.Add("Áreas");
            dt.Columns.Add("V/G");
            dt.Columns.Add("Montos");

            foreach (var obj in lstAreasIE)
            {
                PintarAreasIE(obj, dt);
            }

            GenerarPdf(dt, "Ingresos y Egresos por &Aacute;reas", "IngresosYEgresosPorAreas", objEmpresa, FechaInicio, FechaFin, Response);

            return RedirectToAction("ReporteCategorias", new { message = 2 });
        }
        public ActionResult GenerarRep_FacturacionPorCliente(DateTime? FechaInicio, DateTime? FechaFin)
        {
            if (FechaInicio == null || FechaFin == null)
            {
                return RedirectToAction("ReporteCategorias", new { message = 1 });
            }

            EmpresaDTO objEmpresa = (new EmpresaBL()).getEmpresa(getCurrentUser().IdEmpresa);

            ReportesBL repBL = new ReportesBL();
            List<EntidadResponsableR_DTO> lstClientes = repBL.getFacturacionPorClientes(objEmpresa.IdEmpresa, FechaInicio, FechaFin);

            if (lstClientes == null)
                return RedirectToAction("ReporteCategorias", new { message = 2 });

            System.Data.DataTable dt = new System.Data.DataTable();
            dt.Clear();

            dt.Columns.Add("Clientes");
            dt.Columns.Add("Monto");
            dt.Columns.Add("Porcentaje");

            Decimal SumaTotal = lstClientes.Sum(x => x.Monto);

            foreach (var obj in lstClientes)
            {
                System.Data.DataRow row = dt.NewRow();
                row["Clientes"] = obj.Nombre;
                row["Monto"] = obj.Monto;
                Decimal porcentaje = SumaTotal == 0 ? 0 : obj.Monto / SumaTotal;
                row["Porcentaje"] = porcentaje.ToString("P2", CultureInfo.InvariantCulture);
                dt.Rows.Add(row);
            }

            System.Data.DataRow rowFinal = dt.NewRow();
            rowFinal["Clientes"] = "TOTAL";
            rowFinal["Monto"] = SumaTotal.ToString("N2", CultureInfo.InvariantCulture);
            dt.Rows.Add(rowFinal);

            GenerarPdf(dt, "Ingresos por Clientes", "IngresosPorClientes", objEmpresa, FechaInicio, FechaFin, Response);

            return RedirectToAction("ReporteCategorias", new { message = 2 });
        }
        public ActionResult GenerarRep_GastosPorProveedor(DateTime? FechaInicio, DateTime? FechaFin)
        {
            if (FechaInicio == null || FechaFin == null)
            {
                return RedirectToAction("ReporteCategorias", new { message = 1 });
            }

            EmpresaDTO objEmpresa = (new EmpresaBL()).getEmpresa(getCurrentUser().IdEmpresa);

            ReportesBL repBL = new ReportesBL();
            List<EntidadResponsableR_DTO> lstProveedores = repBL.getGastosPorProveedores(objEmpresa.IdEmpresa, FechaInicio, FechaFin);

            if (lstProveedores == null)
                return RedirectToAction("ReporteCategorias", new { message = 2 });

            System.Data.DataTable dt = new System.Data.DataTable();
            dt.Clear();

            dt.Columns.Add("Proveedores");
            dt.Columns.Add("Monto");
            dt.Columns.Add("Porcentaje");

            Decimal SumaTotal = lstProveedores.Sum(x => x.Monto);

            foreach (var obj in lstProveedores)
            {
                System.Data.DataRow row = dt.NewRow();
                row["Proveedores"] = obj.Nombre;
                row["Monto"] = obj.Monto;
                Decimal porcentaje = SumaTotal == 0 ? 0 : obj.Monto / SumaTotal;
                row["Porcentaje"] = porcentaje.ToString("P2", CultureInfo.InvariantCulture);
                dt.Rows.Add(row);
            }

            System.Data.DataRow rowFinal = dt.NewRow();
            rowFinal["Proveedores"] = "TOTAL";
            rowFinal["Monto"] = SumaTotal.ToString("N2", CultureInfo.InvariantCulture);
            dt.Rows.Add(rowFinal);

            GenerarPdf(dt, "Gastos por Proveedores", "GastosPorProveedores", objEmpresa, FechaInicio, FechaFin, Response);

            return RedirectToAction("ReporteCategorias", new { message = 2 });
        }
        public ActionResult GenerarRep_FacturacionPorVendedor(DateTime? FechaInicio, DateTime? FechaFin)
        {
            if (FechaInicio == null || FechaFin == null)
            {
                return RedirectToAction("ReporteCategorias", new { message = 1 });
            }

            EmpresaDTO objEmpresa = (new EmpresaBL()).getEmpresa(getCurrentUser().IdEmpresa);

            ReportesBL repBL = new ReportesBL();
            List<ResponsableDTO> lstVendedores = repBL.getFacturacionPorVendedores(objEmpresa.IdEmpresa, FechaInicio, FechaFin);

            if (lstVendedores == null)
                return RedirectToAction("ReporteCategorias", new { message = 2 });

            System.Data.DataTable dt = new System.Data.DataTable();
            dt.Clear();

            dt.Columns.Add("Consultores");
            dt.Columns.Add("Monto");
            dt.Columns.Add("Porcentaje");

            Decimal SumaTotal = lstVendedores.Sum(x => x.Monto);

            foreach (var obj in lstVendedores)
            {
                System.Data.DataRow row = dt.NewRow();
                row["Consultores"] = obj.Nombre;
                row["Monto"] = obj.Monto;
                Decimal porcentaje = SumaTotal == 0 ? 0 : obj.Monto / SumaTotal;
                row["Porcentaje"] = porcentaje.ToString("P2", CultureInfo.InvariantCulture);
                dt.Rows.Add(row);
            }

            System.Data.DataRow rowFinal = dt.NewRow();
            rowFinal["Consultores"] = "TOTAL";
            rowFinal["Monto"] = SumaTotal.ToString("N2", CultureInfo.InvariantCulture);
            dt.Rows.Add(rowFinal);

            GenerarPdf(dt, "Ingresos por Consultores", "IngresosPorConsultores", objEmpresa, FechaInicio, FechaFin, Response);

            return RedirectToAction("ReporteCategorias", new { message = 2 });
        }
        public ActionResult GenerarRep_DocumentosIngresoYEgresoPagadosYPorCobrar(int IdTipoComprobante, DateTime? FechaInicio, DateTime? FechaFin)
        {
            if (FechaInicio == null || FechaFin == null)
            {
                return RedirectToAction("ReporteCategorias", new { message = 1 });
            }

            EmpresaDTO objEmpresa = (new EmpresaBL()).getEmpresa(getCurrentUser().IdEmpresa);

            ReportesBL repBL = new ReportesBL();

            List<ComprobanteDTO> lstComprobantes = repBL.getComprobantesIngresosYEgresosEnEmpresa(objEmpresa.IdEmpresa, IdTipoComprobante, FechaInicio.GetValueOrDefault(), FechaFin.GetValueOrDefault());
            //List<ComprobanteDTO> lstPagados = lstComprobantes.Where(x => x.Ejecutado).OrderBy(x => x.FechaEmision).ToList();
            //List<ComprobanteDTO> lstPorCobrar = lstComprobantes.Where(x => !x.Ejecutado).OrderBy(x => x.FechaEmision).ToList();
            if (lstComprobantes == null)
                return RedirectToAction("ReporteCategorias", new { message = 2 });

            System.Data.DataTable dt = new System.Data.DataTable();
            dt.Clear();

            string Entidad = IdTipoComprobante == 1 ? "Cliente" : "Entidad";
            string FechaEjecucion = IdTipoComprobante == 1 ? "Fecha Cobro" : "Fecha Pago";

            dt.Columns.Add("Numero");
            dt.Columns.Add("Documento");
            dt.Columns.Add("Fecha");
            dt.Columns.Add("Status");
            dt.Columns.Add(Entidad);
            if (IdTipoComprobante == 1)
            { dt.Columns.Add("Proyecto"); }
            dt.Columns.Add("Moneda");
            dt.Columns.Add("Monto Sin IGV");
            dt.Columns.Add("Monto Total");
            dt.Columns.Add("Partida de Presupuesto");
            dt.Columns.Add("Monto Pendiente");
            dt.Columns.Add(FechaEjecucion);
            dt.Columns.Add("Dias Vencidos");
            dt.Columns.Add("Comentarios");

            List<bool> Ejecutados = new List<bool>() { true, false };
            DateTime FechaActual = DateTime.Now;


            foreach (var elem in Ejecutados)
            {
                List<ComprobanteDTO> lista = lstComprobantes.Where(x => x.Ejecutado == elem).OrderBy(x => x.FechaEmision).ToList();

                foreach (var obj in lista)
                {
                    System.Data.DataRow row = dt.NewRow();
                    row["Numero"] = obj.NroDocumento;
                    row["Documento"] = obj.NombreTipoDocumento;
                    row["Fecha"] = obj.FechaEmision.ToString("yyyy/MM/dd", CultureInfo.CreateSpecificCulture("es-PE"));
                    row["Status"] = obj.Ejecutado ? "Cancelado" : "Pendiente";
                    row[Entidad] = obj.NombreEntidad;
                    if (IdTipoComprobante == 1)
                    { row["Proyecto"] = obj.NombreProyecto; }
                    row["Moneda"] = obj.SimboloMoneda;
                    row["Monto Sin IGV"] = obj.MontoSinIGV.ToString("N2", CultureInfo.InvariantCulture);
                    row["Monto Total"] = obj.Monto.ToString("N2", CultureInfo.InvariantCulture);
                    row["Partida de Presupuesto"] = obj.NombreCategoria;
                    row["Monto Pendiente"] = obj.Ejecutado ? "0.00" : obj.MontoIncompleto.ToString("N2", CultureInfo.InvariantCulture);
                    row[FechaEjecucion] = obj.FechaConclusion != null ? obj.FechaConclusion.GetValueOrDefault().ToString("yyyy/MM/dd", CultureInfo.CreateSpecificCulture("es-PE")) : "-";
                    row["Dias Vencidos"] = obj.Ejecutado ? "0" : obj.FechaConclusion != null ? (FechaActual - obj.FechaConclusion.GetValueOrDefault()).Days.ToString() : "N/A";
                    row["Comentarios"] = obj.Comentario;
                    dt.Rows.Add(row);
                }
                if (elem != Ejecutados.Last())
                {
                    DataRow space = dt.NewRow();
                    dt.Rows.Add(space);
                }
            }

            string titulo = IdTipoComprobante == 1 ? "Status de Pago de documentos de Ingresos" : "Status de Pago de documentos de Egresos";
            string nombreFile = IdTipoComprobante == 1 ? "StatusDePago_DocsDeIngresos" : "StatusDePago_DocsDeEgresos";

            GenerarPdf2(dt, titulo, nombreFile, objEmpresa, FechaInicio, FechaFin, Response);

            return RedirectToAction("ReporteCategorias", new { message = 2 });
        }
        public ActionResult GenerarRep_DetalleIngresosYGastosPorPartidaDePresupuesto(int IdCategoria, DateTime? FechaInicio, DateTime? FechaFin)
        {
            if (FechaInicio == null || FechaFin == null)
            {
                return RedirectToAction("ReporteCategorias", new { message = 1 });
            }

            EmpresaDTO objEmpresa = (new EmpresaBL()).getEmpresa(getCurrentUser().IdEmpresa);

            ReportesBL repBL = new ReportesBL();

            CategoriaR_DTO catArbol = repBL.getDetalleIngresosYGastos_PorPartidaDePresupuesto(IdCategoria, objEmpresa.IdEmpresa, FechaInicio.GetValueOrDefault(), FechaFin.GetValueOrDefault());

            if (catArbol == null)
                return RedirectToAction("ReporteCategorias", new { message = 2 });

            System.Data.DataTable dt = new System.Data.DataTable();
            dt.Clear();

            dt.Columns.Add("Nivel");
            dt.Columns.Add("Partida");
            dt.Columns.Add("Fecha");
            dt.Columns.Add("Entidad");
            dt.Columns.Add("Documento");
            dt.Columns.Add("# Documento");
            dt.Columns.Add("Moneda");
            dt.Columns.Add("Monto Total");
            dt.Columns.Add("Area(s)");
            dt.Columns.Add("Comentario");

            DataRow rowP = dt.NewRow();
            rowP["Nivel"] = catArbol.Nivel;
            rowP["Partida"] = catArbol.Nombre;
            dt.Rows.Add(rowP);
            foreach (var obj in catArbol.Comprobantes)
            {
                System.Data.DataRow row = dt.NewRow();
                row["Fecha"] = obj.Fecha.ToString("yyyy/MM/dd", CultureInfo.CreateSpecificCulture("es-PE"));
                row["Entidad"] = obj.NombreEntidad;
                row["Documento"] = obj.NombreDocumento;
                row["# Documento"] = obj.NroDocumento;
                row["Moneda"] = obj.Moneda;
                row["Monto Total"] = obj.Monto.ToString("N2", CultureInfo.InvariantCulture);
                row["Area(s)"] = obj.Areas;
                row["Comentario"] = obj.Comentario;

                dt.Rows.Add(row);
            }
            PintarGastoPorPartidaPresupuesto(catArbol.Hijos.ToList(), dt);

            GenerarPdf2(dt, "Detalle de Ingresos y Gastos por Partida de Presupuesto", "DetalleIngresosYGastos_PartidaDePresupuestos", objEmpresa, FechaInicio, FechaFin, Response);

            return RedirectToAction("ReporteCategorias", new { message = 2 });
        }
        public ActionResult GenerarRep_FacturacionPorHonorarios(DateTime? FechaInicio, DateTime? FechaFin)
        {
            if (FechaInicio == null || FechaFin == null)
            {
                return RedirectToAction("ReporteCategorias", new { message = 1 });
            }

            EmpresaDTO objEmpresa = (new EmpresaBL()).getEmpresa(getCurrentUser().IdEmpresa);

            ReportesBL repBL = new ReportesBL();
            List<HonorarioDTO> lstHonorariosMontos = repBL.getHonorariosEnEmpresa(objEmpresa.IdEmpresa, FechaInicio, FechaFin);

            if (lstHonorariosMontos == null)
                return RedirectToAction("ReporteCategorias", new { message = 2 });

            System.Data.DataTable dt = new System.Data.DataTable();
            dt.Clear();

            dt.Columns.Add("Modalidad");
            dt.Columns.Add("Monto");
            dt.Columns.Add("Porcentaje");

            Decimal SumaTotal = lstHonorariosMontos.Sum(x => x.Monto);

            foreach (var obj in lstHonorariosMontos)
            {
                DataRow row = dt.NewRow();
                row["Modalidad"] = obj.Nombre;
                row["Monto"] = obj.Monto.ToString("N2", CultureInfo.InvariantCulture);
                Decimal porcentaje = SumaTotal == 0 ? 0 : obj.Monto / SumaTotal;
                row["Porcentaje"] = porcentaje.ToString("P2", CultureInfo.InvariantCulture);
                dt.Rows.Add(row);
            }

            System.Data.DataRow rowFinal = dt.NewRow();
            rowFinal[0] = "TOTAL";
            rowFinal[2] = SumaTotal.ToString("N2", CultureInfo.InvariantCulture);
            dt.Rows.Add(rowFinal);

            GenerarPdf(dt, "Facturaci&oacute;n por Modalidad de Pago", "FacturacionPorModalidadDePago", objEmpresa, FechaInicio, FechaFin, Response);
            
            return RedirectToAction("ReporteCategorias", new { message = 2 });
        }
        private static void GenerarPdf(DataTable dt, string titulo, string nombreDoc, EmpresaDTO objEmpresa, DateTime? FechaInicio, DateTime? FechaFin, HttpResponseBase Response)
        {
            GridView gv = new GridView();

            gv.DataSource = dt;
            gv.AllowPaging = false;
            gv.DataBind();

            if (dt.Rows.Count > 0)
            {
                PintarCabeceraTabla(gv);
                //PintarIntercaladoCategorias(gv);

                AddSuperHeader(gv, titulo + " - Empresa:" + objEmpresa.Nombre);
                //Cabecera principal
                AddWhiteHeader(gv, 1, "");
                AddWhiteHeader(gv, 2, "PERIODO: " + FechaInicio.GetValueOrDefault().ToShortDateString() + " - " + FechaFin.GetValueOrDefault().ToShortDateString());
                AddWhiteHeader(gv, 3, "Moneda: (" + objEmpresa.SimboloMoneda + ")");

                //PintarCategorias(gv);

                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=" + nombreDoc + "_" + objEmpresa.Nombre + "_" + DateTime.Now.ToString("dd-MM-yyyy") + ".xls");
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
        }
        private static void GenerarPdf2(DataTable dt, string titulo, string nombreDoc, EmpresaDTO objEmpresa, DateTime? FechaInicio, DateTime? FechaFin, HttpResponseBase Response)
        {
            GridView gv = new GridView();

            gv.DataSource = dt;
            gv.AllowPaging = false;
            gv.DataBind();

            if (dt.Rows.Count > 0)
            {
                PintarCabeceraTabla(gv);
                //PintarIntercaladoCategorias(gv);

                AddSuperHeader(gv, titulo);
                //Cabecera principal
                AddWhiteHeader(gv, 1, "");
                AddWhiteHeader(gv, 2, "PERIODO: " + FechaInicio.GetValueOrDefault().ToShortDateString() + " - " + FechaFin.GetValueOrDefault().ToShortDateString());

                //PintarCategorias(gv);

                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=" + nombreDoc + "_" + DateTime.Now.ToString("dd-MM-yyyy") + ".xls");
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
        }
        private static void PintarGastoPorPartidaPresupuesto(List<CategoriaR_DTO> lista, DataTable dt)
        {
            foreach (var obj in lista)
            {
                DataRow row1 = dt.NewRow();
                row1["Nivel"] = obj.Nivel;
                row1["Partida"] = obj.Nombre;
                dt.Rows.Add(row1);
                foreach (var com in obj.Comprobantes)
                {
                    DataRow row2 = dt.NewRow();
                    row2["Fecha"] = com.Fecha.ToString("yyyy/MM/dd", CultureInfo.CreateSpecificCulture("es-PE"));
                    row2["Entidad"] = com.NombreEntidad;
                    row2["Documento"] = com.NombreDocumento;
                    row2["# Documento"] = com.NroDocumento;
                    row2["Moneda"] = com.Moneda;
                    row2["Monto Total"] = com.Monto.ToString("N2", CultureInfo.InvariantCulture);
                    row2["Area(s)"] = com.Areas;
                    row2["Comentario"] = com.Comentario;

                    dt.Rows.Add(row2);
                }
                PintarGastoPorPartidaPresupuesto(obj.Hijos.ToList(), dt);
            }
        }
        private static void PintarArbolPadre(CategoriaDTO obj, List<CategoriaDTO> lstCatMontos, EmpresaDTO objEmpresa, System.Data.DataTable dt)
        {
            System.Data.DataRow row = dt.NewRow();
            row["Nivel"] = obj.Nivel;
            row["P. Presupuesto"] = obj.Nombre;
            Decimal pMonto = lstCatMontos.SingleOrDefault(x => x.IdCategoria == obj.IdCategoria).Presupuesto.GetValueOrDefault();
            row["Total (" + objEmpresa.SimboloMoneda + ")"] = pMonto.ToString("N2", CultureInfo.InvariantCulture);
            row["PRESUPUESTO ANUAL (" + objEmpresa.SimboloMoneda + ")"] = obj.Presupuesto.GetValueOrDefault().ToString("N2", CultureInfo.InvariantCulture);
            Decimal porcentaje = obj.Presupuesto.GetValueOrDefault() != 0 ? pMonto / obj.Presupuesto.GetValueOrDefault() : 0;
            row["PRESUPUESTO EJECUTADO A LA FECHA %"] = Math.Abs(porcentaje).ToString("P2", CultureInfo.InvariantCulture);
            dt.Rows.Add(row);
            foreach (var hijo in obj.Hijos)
            {
                PintarArbolPadre(hijo, lstCatMontos, objEmpresa, dt);
            }
        }
        private static void PintarAreas(AreaDTO obj, Decimal SumaTotal, System.Data.DataTable dt)
        {
            System.Data.DataRow row1 = dt.NewRow();
            row1[0] = obj.Nombre;
            row1["Montos"] = obj.SumaClientes.ToString("N2", CultureInfo.InvariantCulture);
            Decimal porcentaje = SumaTotal == 0 ? 0 : obj.SumaClientes / SumaTotal;
            row1["Porcentaje"] = porcentaje.ToString("P2", CultureInfo.InvariantCulture);
            dt.Rows.Add(row1);

            foreach (var item in obj.lstClientes)
            {
                System.Data.DataRow row2 = dt.NewRow();
                row2[1] = item.Nombre;
                row2["Montos"] = item.Monto.ToString("N2", CultureInfo.InvariantCulture);
                dt.Rows.Add(row2);
            }
        }
        private static void PintarAreasIE(AreaDTO obj, System.Data.DataTable dt)
        {
            DataRow row1 = dt.NewRow();
            row1[0] = obj.Nombre.ToUpper();
            dt.Rows.Add(row1);
            DataRow row2 = dt.NewRow();
            row2[1] = "VENTAS";
            row2[2] = obj.Ingresos.ToString("N2", CultureInfo.InvariantCulture);
            dt.Rows.Add(row2);
            DataRow row3 = dt.NewRow();
            row3[1] = "GASTOS";
            row3[2] = obj.Egresos.ToString("N2", CultureInfo.InvariantCulture);
            dt.Rows.Add(row3);
            DataRow row4 = dt.NewRow();
            row4[0] = "NETO";
            row4[2] = (obj.Ingresos + obj.Egresos).ToString("N2", CultureInfo.InvariantCulture);
            dt.Rows.Add(row4);
        }

        #endregion

        #region Exportar Detalles
        public ActionResult ExportarLibros(int idTipoCuenta, DateTime? FechaInicio, DateTime? FechaFin)
        {
            if (FechaInicio == null || FechaFin == null)
            {
                createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_FILE_DETAIL);
                return RedirectToAction("Libros", "Admin");
            }

            EmpresaDTO objEmpresa = (new EmpresaBL()).getEmpresa(getCurrentUser().IdEmpresa);

            ReportesBL repBL = new ReportesBL();
            List<CuentaBancariaDTO> lstCuentas = repBL.getCuentasBancariasEnEmpresa(objEmpresa.IdEmpresa, idTipoCuenta, FechaInicio.GetValueOrDefault(), FechaFin.GetValueOrDefault());

            if (lstCuentas == null || lstCuentas.Count == 0)
            {
                createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_EMPTY);
                return RedirectToAction("Libros", "Admin");
            }

            System.Data.DataTable dt = new System.Data.DataTable();
            dt.Clear();

            dt.Columns.Add("Nombre");
            dt.Columns.Add("Fecha");
            dt.Columns.Add("Moneda");
            string miSaldo = idTipoCuenta == 1 ? "Saldo Disponible" : "Saldo Total";
            dt.Columns.Add(miSaldo);
            if (idTipoCuenta == 1) { dt.Columns.Add("Saldo Bancario"); }
            dt.Columns.Add("Estado");

            foreach (var obj in lstCuentas)
            {
                System.Data.DataRow row = dt.NewRow();
                row["Nombre"] = obj.NombreCuenta;
                row["Fecha"] = obj.FechaConciliacion.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);
                row["Moneda"] = obj.SimboloMoneda;
                row[3] = obj.SaldoDisponible.ToString("N2", CultureInfo.InvariantCulture);
                if (idTipoCuenta == 1) { row["Saldo Bancario"] = obj.SaldoBancario.ToString("N2", CultureInfo.CreateSpecificCulture("es-PE")); }
                row["Estado"] = obj.Estado ? "Activo" : "Inactivo";
                dt.Rows.Add(row);
            }

            GenerarPdf(dt, "Detalle de Libros", "DetalleLibros", objEmpresa, FechaInicio, FechaFin, Response);

            createResponseMessage(CONSTANTES.SUCCESS, CONSTANTES.SUCCESS_FILE);
            return RedirectToAction("Libros", "Admin");
        }

        public ActionResult ExportarMovimientos(int idLibro, DateTime? FechaInicio, DateTime? FechaFin)
        {
            if (FechaInicio == null || FechaFin == null)
            {
                createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_FILE_DETAIL);
                return RedirectToAction("Libros", "Admin");
            }

            EmpresaDTO objEmpresa = (new EmpresaBL()).getEmpresa(getCurrentUser().IdEmpresa);

            ReportesBL repBL = new ReportesBL();
            CuentaBancariaDTO CuentaBancaria = repBL.getCuentaBancaria(idLibro, FechaInicio.GetValueOrDefault(), FechaFin.GetValueOrDefault());

            if (CuentaBancaria == null || CuentaBancaria.listaMovimiento.Count == 0)
            {
                createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_EMPTY);
                return RedirectToAction("Libros", "Admin");
            }

            System.Data.DataTable dt = new System.Data.DataTable();
            dt.Clear();

            dt.Columns.Add("Fecha");
            dt.Columns.Add("Movimiento");
            dt.Columns.Add("Detalle");
            dt.Columns.Add("Monto");
            dt.Columns.Add("Partida de Presupuesto");
            dt.Columns.Add("Entidad");
            dt.Columns.Add("Numero de documento");
            dt.Columns.Add("Estado");
            dt.Columns.Add("Usuario");
            dt.Columns.Add("Activo");
            dt.Columns.Add("Comentario");

            foreach (var obj in CuentaBancaria.listaMovimiento)
            {
                System.Data.DataRow row = dt.NewRow();
                row["Fecha"] = obj.Fecha.ToString("yyyy/MM/dd", CultureInfo.CreateSpecificCulture("es-PE")); ;
                row["Movimiento"] = obj.IdTipoMovimiento == 1 ? "Entrada" : "Salida";
                row["Detalle"] = obj.NroOperacion;
                row["Monto"] = CuentaBancaria.SimboloMoneda + obj.Monto.ToString("N2", CultureInfo.InvariantCulture);
                row["Partida de Presupuesto"] = obj.NombreCategoria == null ? "N/A" : obj.NombreCategoria;
                row["Entidad"] = obj.NombreEntidadR;
                row["Numero de documento"] = obj.NumeroDocumento != null ? obj.NumeroDocumento : obj.NumeroDocumento2 != null ? obj.NumeroDocumento2 : "N/A";
                row["Estado"] = obj.IdEstadoMovimiento == 1 ? "Pendiente" : "Realizado";
                row["Usuario"] = obj.NombreUsuario;
                row["Activo"] = obj.Estado ? "Activo" : "Inactivo";
                row["Comentario"] = obj.Comentario;
                dt.Rows.Add(row);
            }

            GenerarPdf(dt, "Detalle de Movimientos en la Cuenta - " + CuentaBancaria.NombreCuenta, "DetalleMovimientosEnLibro", objEmpresa, FechaInicio, FechaFin, Response);

            createResponseMessage(CONSTANTES.SUCCESS, CONSTANTES.SUCCESS_FILE);
            return RedirectToAction("Libros", "Admin");
        }

        public ActionResult ExportarComprobantes(int idTipoComprobante, DateTime? FechaInicio, DateTime? FechaFin)
        {
            if (FechaInicio == null || FechaFin == null)
            {
                createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_FILE_DETAIL);
                return RedirectToAction("Libros", "Admin");
            }

            EmpresaDTO objEmpresa = (new EmpresaBL()).getEmpresa(getCurrentUser().IdEmpresa);

            ReportesBL repBL = new ReportesBL();
            List<ComprobanteDTO> lstComprobantes = repBL.getComprobantesEnEmpresa(objEmpresa.IdEmpresa, idTipoComprobante, FechaInicio.GetValueOrDefault(), FechaFin.GetValueOrDefault());

            if (lstComprobantes == null || lstComprobantes.Count == 0)
            {
                createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_EMPTY);
                return RedirectToAction("Comprobantes", "Admin");
            }

            System.Data.DataTable dt = new System.Data.DataTable();
            dt.Clear();

            var rFechaFin = idTipoComprobante == 1 ? "Fecha de Cobro" : "Fecha de Pago";
            var strEntidad = idTipoComprobante == 1 ? "Cliente" : "Proveedor";

            dt.Columns.Add("Fecha");
            dt.Columns.Add("Documento");
            dt.Columns.Add("Numero");
            dt.Columns.Add(strEntidad);
            if (idTipoComprobante == 1)
            { dt.Columns.Add("Proyecto"); }
            dt.Columns.Add("Moneda");
            dt.Columns.Add("Monto Sin IGV");
            dt.Columns.Add("Partida de Presupuesto");
            dt.Columns.Add(rFechaFin);
            dt.Columns.Add("Usuario");
            dt.Columns.Add("Estado");
            dt.Columns.Add("Status");
            dt.Columns.Add("Comentario");

            foreach (var obj in lstComprobantes)
            {
                System.Data.DataRow row = dt.NewRow();
                row["Fecha"] = obj.FechaEmision.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);
                row["Documento"] = obj.NombreTipoDocumento;
                row["Numero"] = obj.NroDocumento;
                row[strEntidad] = obj.NombreEntidad;
                if (idTipoComprobante == 1) { row["Proyecto"] = obj.NombreProyecto; }
                row["Moneda"] = obj.SimboloMoneda;
                row["Monto Sin IGV"] = obj.MontoSinIGV.ToString("N2", CultureInfo.InvariantCulture);
                row["Partida de Presupuesto"] = obj.NombreCategoria;
                row[rFechaFin] = obj.FechaConclusion.GetValueOrDefault().ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);
                row["Usuario"] = obj.NombreUsuario;
                row["Estado"] = obj.Estado ? "Activo" : "Inactivo";
                row["Status"] = obj.Ejecutado ? "Cancelado" : "Pendiente";
                row["Comentario"] = obj.Comentario;
                dt.Rows.Add(row);
            }

            GenerarPdf(dt, "Detalle de Comprobantes", "DetalleComprobantes", objEmpresa, FechaInicio, FechaFin, Response);

            createResponseMessage(CONSTANTES.SUCCESS, CONSTANTES.SUCCESS_FILE);
            return RedirectToAction("Comprobantes", "Admin");
        }

        #endregion
        /*
        private static System.Data.DataRow DameRowPintarPadres(System.Data.DataRow row, CategoriaR_DTO categoria)
        {
            if (categoria.Padre != null)
            {
                row = DameRowPintarPadres(row, categoria.Padre);
            }

            switch (categoria.Nivel)
            {
                case 0:
                    row["Categoria"] = categoria.Nombre;
                    break;
                default:
                    row["Categoria Sub " + categoria.Nivel.ToString()] = categoria.Nombre;
                    break;
            }
            return row;
        }*/
        private static void AddSuperHeader(GridView gridView, string text = null)
        {
            var myTable = (Table)gridView.Controls[0];
            var myNewRow = new GridViewRow(0, -1, DataControlRowType.Header, DataControlRowState.Normal);
            myNewRow.Cells.Add(MakeCell(text, gridView.HeaderRow.Cells.Count));//gridView.Columns.Count
            myNewRow.Cells[0].Style.Add("background-color", "#cbcfd6");
            myTable.Rows.AddAt(0, myNewRow);
            //myTable.EnableViewState = false;
        }
        private static void AddHeader(GridView gridView, int index, string text = null)
        {
            var myTable = (Table)gridView.Controls[0];
            var myNewRow = new GridViewRow(0, -1, DataControlRowType.Header, DataControlRowState.Normal);
            myNewRow.Cells.Add(MakeCell(text, gridView.HeaderRow.Cells.Count));//gridView.Columns.Count
            myNewRow.Cells[0].Style.Add("background-color", "#cbcfd6");
            myNewRow.Cells[0].HorizontalAlign = HorizontalAlign.Left;
            myTable.Rows.AddAt(index, myNewRow);
            //myTable.EnableViewState = false;
        }
        private static void AddWhiteHeader(GridView gridView, int index, string text = null)
        {
            var myTable = (Table)gridView.Controls[0];
            var myNewRow = new GridViewRow(0, -1, DataControlRowType.Header, DataControlRowState.Normal);
            myNewRow.Cells.Add(MakeCell(text, gridView.HeaderRow.Cells.Count));//gridView.Columns.Count
            myNewRow.Cells[0].Style.Add("background-color", "#ffffff");
            myNewRow.Cells[0].HorizontalAlign = HorizontalAlign.Left;
            myTable.Rows.AddAt(index, myNewRow);
        }
        private static void PintarCabeceraTabla(GridView gridView)
        {
            var myTable = (Table)gridView.Controls[0];
            foreach (GridViewRow row in myTable.Rows)
            {
                if (row.Cells.Count >= 3)
                {
                    for (int i = 0; i < row.Cells.Count; i++)
                    {
                        row.Cells[i].Text = row.Cells[i].Text.ToUpper();
                        row.Cells[i].BackColor = System.Drawing.Color.FromArgb(189, 195, 199);
                        row.Cells[i].Font.Bold = true;
                    }
                    break;
                }
            }
        }
        private static void PintarIntercaladoCategorias(GridView gridView)
        {
            var myTable = (Table)gridView.Controls[0];

            //Colores
            System.Drawing.Color colorPadre = new System.Drawing.Color();
            colorPadre = System.Drawing.Color.FromArgb(255, 255, 255);
            System.Drawing.Color colorSub = new System.Drawing.Color();
            colorSub = System.Drawing.Color.FromArgb(255, 255, 255);

            bool blancoActive = true;
            bool blancoActive2 = true;
            string cadenaPadre = "";
            string cadenaSub = "";

            int contPadre = 0;
            foreach (GridViewRow row in myTable.Rows)
            {
                if (cadenaPadre != row.Cells[0].Text)
                {
                    if (!blancoActive)
                    {
                        colorPadre = System.Drawing.Color.FromArgb(229, 231, 235);
                        blancoActive = true;
                    }
                    else
                    {
                        colorPadre = System.Drawing.Color.FromArgb(255, 255, 255);
                        blancoActive = false;
                    }
                    cadenaPadre = row.Cells[0].Text;
                }
                for (int i = 0; i < row.Cells.Count; i++)
                {
                    if (i == 0)
                    {
                        row.Cells[i].BackColor = colorPadre;
                    }
                    else
                    {
                        if (i == 1)
                        {
                            if (cadenaSub != row.Cells[i].Text)
                            {
                                if (!blancoActive2)
                                {
                                    colorSub = System.Drawing.Color.FromArgb(229, 231, 235);
                                    blancoActive2 = true;
                                }
                                else
                                {
                                    colorSub = System.Drawing.Color.FromArgb(255, 255, 255);
                                    blancoActive2 = false;
                                }
                                cadenaSub = row.Cells[i].Text;
                            }
                        }
                        row.Cells[i].BackColor = colorSub;
                    }
                }
                contPadre++;
            }
        }

        private static void PintarColumnaNegrita(GridView gridView, int columna, bool intercalado)
        {
            var myTable = (Table)gridView.Controls[0];
            bool impar = false;
            foreach (GridViewRow row in myTable.Rows)
            {
                if (row.Cells[columna].Text != null)
                {
                    row.Cells[columna].Font.Bold = true;
                }
                if (intercalado)
                {
                    if (impar)
                    {
                        for (int i = 0; i < row.Cells.Count; i++)
                        {
                            row.Cells[i].BackColor = System.Drawing.Color.FromArgb(229, 231, 235);
                        }
                        impar = false;
                    }
                    else
                    {
                        impar = true;
                    }
                }
            }
        }

        private static void PintarCategorias(GridView gridView)
        {
            var myTable = (Table)gridView.Controls[0];

            foreach (GridViewRow row in myTable.Rows)
            {
                if (row.Cells.Count >= 2)
                {
                    string cadena0 = row.Cells[0].Text;
                    string cadena1 = row.Cells[1].Text;
                    if (cadena0 == "TOTAL :")
                    {
                        for (int i = 0; i < row.Cells.Count; i++)
                        {
                            row.Cells[i].BackColor = System.Drawing.Color.FromArgb(71, 229, 199);
                            row.Cells[i].Font.Bold = true;
                        }
                    }
                    if (cadena1 == "TOTAL :")
                    {
                        for (int i = 0; i < row.Cells.Count; i++)
                        {
                            if (i == 0)
                            {
                                row.Cells[i].BackColor = System.Drawing.Color.FromArgb(255, 255, 255);
                                row.Cells[i].Font.Bold = true;
                            }
                            else
                            {
                                row.Cells[i].BackColor = System.Drawing.Color.FromArgb(95, 174, 227);
                                row.Cells[i].Font.Bold = true;
                            }
                        }
                    }
                }
            }
        }

        private static TableHeaderCell MakeCell(string text = null, int span = 1)
        {
            return new TableHeaderCell() { ColumnSpan = span, Text = text ?? string.Empty, CssClass = "table-header" };
        }

        private static Decimal DameTotalSoles(List<CuentaBancariaDTO> listaLibros)
        {
            Decimal Total = 0;
            foreach (var libro in listaLibros)
            {
                if (libro.IdMoneda.GetValueOrDefault() == 1 && libro.Estado && libro.IdTipoCuenta == 1) //Soles
                {
                    Total += libro.SaldoDisponible;
                }
            }
            return Total;
        }
        private static Decimal DameTotalDolares(List<CuentaBancariaDTO> listaLibros)
        {
            Decimal Total = 0;
            foreach (var libro in listaLibros)
            {
                if (libro.IdMoneda.GetValueOrDefault() == 2 && libro.Estado && libro.IdTipoCuenta == 1) //Dolares
                {
                    Total += libro.SaldoDisponible;
                }
            }
            return Total;
        }
        private static Decimal DameTotalConsolidado(List<CuentaBancariaDTO> listaLibros, Decimal TipoCambio)
        {
            Decimal Total = 0;

            foreach (var libro in listaLibros)
            {
                if (libro.IdMoneda.GetValueOrDefault() == 1 && libro.Estado && libro.IdTipoCuenta == 1) //Soles
                {
                    Total += libro.SaldoDisponible;
                }
            }
            foreach (var libro in listaLibros)
            {
                if (libro.IdMoneda.GetValueOrDefault() == 2 && libro.Estado && libro.IdTipoCuenta == 1) //Dolares
                {
                    Total += libro.SaldoDisponible * TipoCambio;
                }
            }

            return Total;
        }

        [HttpPost]
        public string CambiarEmpresaSuperAdmin(int idEmpresa)
        {
            if (!this.currentUser() || !isSuperAdministrator()) { return "false"; }

            UsuariosBL objBL = new UsuariosBL();
            if (objBL.actualizarEmpresaSuperAdmin(getCurrentUser().IdUsuario, idEmpresa))
            {
                System.Web.HttpContext.Current.Session["User"] = objBL.getUsuario(getCurrentUser().IdUsuario);
                return "true";
            }
            return "false";
        }

        [HttpPost]
        public string ActualizarTipoCambio(Decimal tipoCambio)
        {
            if (!this.currentUser() || !isAdministrator()) { return "false"; }

            EmpresaBL objBL = new EmpresaBL();
            UsuarioDTO miUsuario = getCurrentUser();
            EmpresaDTO obj = new EmpresaDTO() { IdEmpresa = miUsuario.IdEmpresa, TipoCambio = tipoCambio };
            if (objBL.updateTipoCambio(obj))
            {
                return "true";
            }
            return "false";
        }

        [HttpPost]
        public ActionResult PasslstAreasXMontos(List<AreaPorComprobanteDTO> lista)
        {
            TempData["AreasXMontos"] = lista;
            return Json(new { success = true, mensaje = "Si funciona" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public string ActualizarEjecucionComprobante(int idComprobante, bool ejecutado)
        {
            if (!this.currentUser() || isUsuarioExterno()) { return "false"; }

            ComprobanteBL objBL = new ComprobanteBL();
            if (objBL.actualizarEjecutado(idComprobante, ejecutado, getCurrentUser().IdEmpresa))
            { return "true"; }
            return "false";
        }

        public void MenuNavBarSelected(int num, int? subNum = null)
        {
            navbar.clearAll();
            if (num != 0) { navbar.lstOptions[num - 1].cadena = "active"; }

            if (subNum != null && subNum != 0)
            {
                //Limpiar Activos del ultimo elemento
                foreach (var item in navbar.lstOptions.Last().lstOptions)
                {
                    item.cadena = "";
                }
                navbar.lstOptions.Last().lstOptions[subNum.GetValueOrDefault() - 1].cadena = "active";
            }
            ViewBag.navbar = navbar;
        }
    }
}
