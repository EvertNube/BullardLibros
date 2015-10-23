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
                ViewBag.Title = "NubeLabs SCI";

                ViewBag.EsAdmin = isAdministrator();
                ViewBag.EsSuperAdmin = isSuperAdministrator();
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

        [HttpPost]
        public ActionResult Login(UsuarioDTO user)
        {
            UsuariosBL usuariosBL = new UsuariosBL();
            if (usuariosBL.isValidUser(user))
            {
                System.Web.HttpContext.Current.Session["User"] = usuariosBL.getUsuarioByCuenta(user);
                return RedirectToAction("Index");
            }
            else
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
            if(id != null)
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

            MenuNavBarSelected(1);

            UsuarioDTO miUsuario = getCurrentUser();
            
            CuentaBancariaBL objBL = new CuentaBancariaBL();
            ViewBag.lstTipoCuentas = objBL.getTipoDeCuentas();
            ViewBag.idTipoCuenta = idTipoCuenta;
            List<CuentaBancariaDTO> listaLibros = new List<CuentaBancariaDTO>();

            if(miUsuario.IdEmpresa != 0)
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

        public ActionResult Libro(int? id = null, int? idTipoCuenta = null)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            if (!this.isAdministrator()) { return RedirectToAction("Index"); }
            
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
                obj = objBL.getCuentaBancaria((int)id);
                if (obj == null) return RedirectToAction("Index");
                if (obj.IdEmpresa != miUsuario.IdEmpresa) return RedirectToAction("Index");

                /*int pageSize = 100;
                int pageNumber = (page ?? 1);
                //Guardar Paginado
                TempData["PagMovs"] = (page ?? 1);
                obj.listaMovimientoPL = obj.listaMovimiento.ToPagedList(pageNumber, pageSize);*/
                //(int? id = null, int? idTipoComprobante = null)
                return View(obj);
            }

            obj = new CuentaBancariaDTO();
            obj.FechaConciliacion = DateTime.Now;
            obj.IdEmpresa = miUsuario.IdEmpresa;
            if (idTipoCuenta != null && idTipoCuenta != 0) obj.IdTipoCuenta = idTipoCuenta.GetValueOrDefault();

            return View(obj);
        }

        [HttpPost]
        public ActionResult AddLibro(CuentaBancariaDTO dto)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
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

        public ActionResult LibroVista(int? id = null, int? page = null)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }

            MenuNavBarSelected(1);

            CuentaBancariaBL objBL = new CuentaBancariaBL();
            ViewBag.IdCuentaBancaria = id;
            ViewBag.Monedas = objBL.getMonedasBag(false);

            if (id != null)
            {
                CuentaBancariaDTO obj = objBL.getCuentaBancaria((int)id);
                if (obj == null) return RedirectToAction("Index");
                if (obj.IdEmpresa != getCurrentUser().IdEmpresa) return RedirectToAction("Index");

                int pageSize = 100;
                int pageNumber = (page ?? 1);
                //Guardar Paginado
                TempData["PagMovs"] = (page ?? 1);

                obj.listaMovimientoPL = obj.listaMovimiento.ToPagedList(pageNumber, pageSize);
                return View(obj);
            }
            return View();
        }

        public ActionResult Categorias()
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            if (!isAdministrator()) { return RedirectToAction("Index"); }

            MenuNavBarSelected(4, 3);
            EmpresaBL empBL = new EmpresaBL();

            UsuarioDTO miUsuario = getCurrentUser();
            int vPeriodo = empBL.getEmpresa(miUsuario.IdEmpresa).IdPeriodo.GetValueOrDefault();
            ViewBag.IdPeriodo = vPeriodo;

            CategoriaBL objBL = new CategoriaBL();
            ViewBag.Periodos = objBL.GetPeriodosEnEmpresaViewBag(miUsuario.IdEmpresa);
            List<CategoriaDTO> listaCategorias = new List<CategoriaDTO>();
            if(miUsuario.IdEmpresa > 0)
            {
                listaCategorias = objBL.getCategoriasTreeEnEmpresa(miUsuario.IdEmpresa);
            }
            
            return View(listaCategorias);
        }

        public ActionResult Categoria(int? id = null, int? idPadre = null)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            //if (!this.isAdministrator()) { return RedirectToAction("Index"); }
            MenuNavBarSelected(4, 3);
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
            MenuNavBarSelected(1);
            UsuarioDTO miUsuario = getCurrentUser();

            MovimientoBL objBL = new MovimientoBL();
            ViewBag.IdMovimiento = id;
            ViewBag.EstadosMovimientos = objBL.getEstadosMovimientos(false);
            ViewBag.lstFormaMovs = objBL.getListaFormaDeMovimientos();
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
            try
            {
                MovimientoBL objBL = new MovimientoBL();
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
                        //objBL.ActualizarSaldos(dto.IdCuentaBancaria);
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

        public ActionResult Usuarios()
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            if (!this.isAdministrator()) { return RedirectToAction("Index"); }

            MenuNavBarSelected(4, 6);
            UsuarioDTO currentUser = getCurrentUser();

            UsuariosBL usuariosBL = new UsuariosBL();
            List<UsuarioDTO> listaUsuarios = new List<UsuarioDTO>();
            if(currentUser.IdEmpresa > 0)
            {
                listaUsuarios = usuariosBL.getUsuariosEnEmpresa(currentUser.IdEmpresa, currentUser.IdRol);
            }

            return View(listaUsuarios);
        }

        public ActionResult Usuario(int? id = null)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }

            MenuNavBarSelected(4, 6);

            UsuarioDTO currentUser = getCurrentUser();
            //if (!this.isAdministrator() && id != currentUser.IdUsuario) { return RedirectToAction("Index"); }
            if (!this.isSuperAdministrator() && id != currentUser.IdUsuario) { return RedirectToAction("Index"); }
            //if (id == 1 && !this.isSuperAdministrator()) { return RedirectToAction("Index"); }
            if (id == 1 && currentUser.IdUsuario != 1) { return RedirectToAction("Index"); }
            UsuariosBL usuariosBL = new UsuariosBL();

            //ViewBag.vbRoles = usuariosBL.getRolesViewBag(true);
            ViewBag.vbRls = usuariosBL.getRolesViewBag(false);

            var objSent = TempData["Usuario"];
            if (objSent != null) { TempData["Usuario"] = null; return View(objSent); }
            UsuarioDTO usuario;
            if (id != null)
            {
                usuario = usuariosBL.getUsuarioEnEmpresa(currentUser.IdEmpresa, id.GetValueOrDefault());
                if (usuario == null) return RedirectToAction("Usuarios");
                if (usuario.IdEmpresa != currentUser.IdEmpresa) return RedirectToAction("Usuarios");
                
                if (usuario.IdRol == 1)
                    ViewBag.vbRls = usuariosBL.getAllRolesViewBag(false);

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
                        if (getCurrentUser().IdRol == 1)
                            return RedirectToAction("Usuarios");
                        else
                            return RedirectToAction("Index");
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
                    createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_INSERT_ACCOUNT);
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
            if (!isAdministrator()) { return RedirectToAction("Index"); }

            MenuNavBarSelected(4, 4);
            UsuarioDTO currentUser = getCurrentUser();

            EntidadResponsableBL objBL = new EntidadResponsableBL();
            ViewBag.idTipoEntidad = idTipoEntidad;
            List<EntidadResponsableDTO> listaEntidades = new List<EntidadResponsableDTO>();
            ViewBag.lstTipoEntidades = objBL.getTipoDeEntidades();
            
            if(currentUser.IdEmpresa > 0)
            {
                listaEntidades = objBL.getEntidadResponsablesEnEmpresa(currentUser.IdEmpresa);
            }
            return View(listaEntidades);
        }

        public ActionResult Entidad(int? id = null, int? idTipoEntidad = null)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            //if (!this.isAdministrator()) { return RedirectToAction("Index"); }
            MenuNavBarSelected(4, 4);
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
        public ActionResult Comprobantes(int? idTipoComprobante = null)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            if (!isAdministrator()) { return RedirectToAction("Index"); }

            MenuNavBarSelected(2);
            UsuarioDTO currentUser = getCurrentUser();

            ComprobanteBL objBL = new ComprobanteBL();
            List<ComprobanteDTO> listaComprobantes = new List<ComprobanteDTO>();
            ViewBag.lstTipoComprobantes = objBL.getTipoDeComprobantes();
            ViewBag.idTipoComprobante = idTipoComprobante;

            if (currentUser.IdEmpresa > 0)
            {
                listaComprobantes = objBL.getComprobantesEnEmpresa(currentUser.IdEmpresa);
            }
            return View(listaComprobantes);
        }
        public ActionResult Comprobante(int? id = null, int? idTipoComprobante = null)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            if (!this.isAdministrator()) { return RedirectToAction("Index"); }
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
                ViewBag.Montos = obj.lstMontos;

                return View(obj);
            }
            obj = new ComprobanteDTO();
            obj.IdEmpresa = currentUser.IdEmpresa;
            obj.TipoCambio = (new EmpresaBL()).getEmpresa(currentUser.IdEmpresa).TipoCambio;
            obj.FechaEmision = DateTime.Now;
            if (idTipoComprobante != null && idTipoComprobante != 0) obj.IdTipoComprobante = idTipoComprobante.GetValueOrDefault();
            return View(obj);
        }
        [HttpPost]
        public ActionResult AddComprobante(ComprobanteDTO dto)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
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

        public ActionResult Areas()
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            if (!isAdministrator()) { return RedirectToAction("Index"); }

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

            MenuNavBarSelected(4, 5);
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
            MenuNavBarSelected(4, 5);
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

            MenuNavBarSelected(4, 2);
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
            MenuNavBarSelected(4, 2);
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
            MenuNavBarSelected(4, 4);
            UsuarioDTO miUsuario = getCurrentUser();

            ProyectoBL objBL = new ProyectoBL();
            ViewBag.IdProyecto = id;

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

        public ActionResult Periodos()
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            if (!isAdministrator()) { return RedirectToAction("Index"); }

            MenuNavBarSelected(4, 7);

            UsuarioDTO currentUser = getCurrentUser();

            PeriodoBL objBL = new PeriodoBL();
            List<PeriodoDTO> listaPeriodos = new List<PeriodoDTO>();

            if(currentUser.IdEmpresa > 0)
            {
                listaPeriodos = objBL.getPeriodosEnEmpresa(currentUser.IdEmpresa);
            }
            return View(listaPeriodos);
        }

        public ActionResult Periodo(int? id = null)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            if (!this.isAdministrator()) { return RedirectToAction("Index"); }
            MenuNavBarSelected(4, 7);
            UsuarioDTO currentUser = getCurrentUser();

            PeriodoBL objBL = new PeriodoBL();

            var objSent = TempData["Periodo"];
            if (objSent != null) { TempData["Periodo"] = null; return View(objSent); }

            PeriodoDTO obj;
            if(id != null)
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
            var listaProyectos = objBL.getProyectosPorEntidad(idEntidad);
            return Json(new { listaProyectos }, JsonRequestBehavior.AllowGet);
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

        public JsonResult BuscarComprobante(int idComprobante)
        {
            ComprobanteBL objBL = new ComprobanteBL();
            var comprobante = objBL.getComprobanteEnEmpresa(getCurrentUser().IdEmpresa, idComprobante);
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

            MenuNavBarSelected(3);

            CuentaBancariaBL objBL = new CuentaBancariaBL();
            ViewBag.Libros = objBL.getCuentasBancariasEnEmpresaBag((int)getCurrentUser().IdEmpresa, true);

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

        private static System.Data.DataRow DameRowPintarPadres(System.Data.DataRow row, CategoriaR_DTO categoria)
        {
            if (categoria.Padre != null)
            {
                row = DameRowPintarPadres(row, categoria.Padre);
            }
            //row["Categoria N." + categoria.Nivel.ToString()] = categoria.Nombre;
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
        }

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
                /*if (contPadre > 0)
                {
                    if (contPadre == 1 || contPadre == 2)
                    {
                        cadenaPadre = row.Cells[0].Text;
                        cadenaSub = row.Cells[1].Text;
                        for (int i = 1; i < row.Cells.Count; i++)
                        {
                            row.Cells[i].BackColor = System.Drawing.Color.FromArgb(229, 231, 235);
                        }
                    }
                    else
                    {*/
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
                    /*}
                }*/
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
                        //row.BackColor = System.Drawing.Color.FromArgb(230, 126, 34);
                    }
                    if (cadena1 == "TOTAL :")
                    {
                        for (int i = 0; i < row.Cells.Count; i++)
                        {
                            if(i == 0)
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
                        //row.BackColor = System.Drawing.Color.FromArgb(52, 152, 219);
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
            if(objBL.actualizarEmpresaSuperAdmin(getCurrentUser().IdUsuario, idEmpresa))
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
            EmpresaDTO obj = new EmpresaDTO(){ IdEmpresa = miUsuario.IdEmpresa, TipoCambio = tipoCambio };
            if(objBL.updateTipoCambio(obj))
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

        public void MenuNavBarSelected(int num, int? subNum = null)
        {
            navbar.clearAll();
            if(num != 0) { navbar.lstOptions[num-1].cadena = "active"; }

            if (subNum != null && subNum != 0)
            {
                //Limpiar Activos del ultimo elemento
                foreach (var item in navbar.lstOptions.Last().lstOptions)
                {
                    item.cadena = "";
                }
                navbar.lstOptions.Last().lstOptions[subNum.GetValueOrDefault()-1].cadena = "active";
            }
            ViewBag.navbar = navbar;
        }
    }
}
