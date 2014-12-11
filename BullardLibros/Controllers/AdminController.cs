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
using PagedList;

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

        public ActionResult Libro(int? id = null)
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
                    if(objBL.add(dto))
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
            catch(Exception e)
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
            return View(objBL.getCategorias());
        }

        public ActionResult Categoria(int? id = null)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            //if (!this.isAdministrator()) { return RedirectToAction("Index"); }
            CategoriaBL objBL = new CategoriaBL();
            ViewBag.IdCategoria = id;
            var objSent = TempData["Categoria"];
            if (objSent != null) { TempData["Categoria"] = null; return View(objSent); }
            if (id != null)
            {
                CategoriaDTO obj = objBL.getCategoria((int)id);
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

        public ActionResult Movimiento(int? id = null)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            //if (!this.isAdministrator()) { return RedirectToAction("Index"); }
            MovimientoBL objBL = new MovimientoBL();
            ViewBag.IdMovimiento = id;
            var objSent = TempData["Movimiento"];
            if (objSent != null) { TempData["Movimiento"] = null; return View(objSent); }
            if (id != null)
            {
                MovimientoDTO obj = objBL.getMovimiento((int)id);
                return View(obj);
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
                        createResponseMessage(CONSTANTES.SUCCESS);
                        return RedirectToAction("Movimientos");
                    }
                }
                else if (dto.IdMovimiento != 0)
                {
                    if (objBL.update(dto))
                    {
                        createResponseMessage(CONSTANTES.SUCCESS);
                        return RedirectToAction("Movimientos");
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
    }
}
