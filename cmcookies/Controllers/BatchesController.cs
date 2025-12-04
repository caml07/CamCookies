using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using cmcookies.Models;
using cmcookies.Services;
using cmcookies.Models.ViewModels.Batch;

namespace cmcookies.Controllers;

[Authorize(Roles = "Admin")]
public class BatchesController : Controller
{
  private readonly CmcDBContext _context;
  private readonly IBatchService _batchService;

  public BatchesController(CmcDBContext context, IBatchService batchService)
  {
    _context = context;
    _batchService = batchService;
  }

  // GET: Batches (Historial)
  public async Task<IActionResult> Index()
  {
    var batches = await _context.Batches
      .Include(b => b.Cookie)
      .OrderByDescending(b => b.ProducedAt)
      .ToListAsync();
    return View(batches);
  }

  // GET: Batches/Create
  public IActionResult Create()
  {
    var viewModel = new BatchCreateViewModel
    {
      CookiesList = new SelectList(_context.Cookies.Where(c => c.IsActive == true), "CookieCode", "CookieName")
    };
    return View(viewModel);
  }

  // POST: Batches/Create
  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Create(BatchCreateViewModel viewModel)
  {
    if (ModelState.IsValid)
      try
      {
        // CORRECCIÓN: viewModel.CookieCode ya es string, así que esto funcionará
        await _batchService.CreateBatchAsync(viewModel.CookieCode);
        TempData["SuccessMessage"] = "¡Batch horneado exitosamente!";
        return RedirectToAction(nameof(Index));
      }
      catch (Exception ex)
      {
        ModelState.AddModelError("", ex.Message);
      }

    // Recargar lista si falló
    viewModel.CookiesList = new SelectList(_context.Cookies.Where(c => c.IsActive == true), "CookieCode", "CookieName",
      viewModel.CookieCode);
    return View(viewModel);
  }
}