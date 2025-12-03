using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using cmcookies.Models;
using cmcookies.Models.ViewModels.Material;

namespace cmcookies.Controllers;

[Authorize(Roles = "Admin")]
public class MaterialsController : Controller
{
  private readonly CmcDBContext _context;

  public MaterialsController(CmcDBContext context)
  {
    _context = context;
  }

  // GET: Materials
  public async Task<IActionResult> Index()
  {
    var materials = await _context.Materials.ToListAsync();
    return View(materials);
  }

  // GET: Materials/Create
  public IActionResult Create()
  {
    return View();
  }

  // POST: Materials/Create
  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Create(MaterialViewModel viewModel)
  {
    if (ModelState.IsValid)
    {
      var material = new Material
      {
        Name = viewModel.Name,
        Unit = viewModel.Unit,
        Stock = viewModel.Stock,
        UnitCost = viewModel.UnitCost,
        CreatedAt = DateTime.Now,
        UpdatedAt = DateTime.Now
      };

      _context.Add(material);
      await _context.SaveChangesAsync();

      TempData["SuccessMessage"] = "Material creado correctamente.";
      return RedirectToAction(nameof(Index));
    }

    return View(viewModel);
  }

  // GET: Materials/Edit/5
  public async Task<IActionResult> Edit(int? id)
  {
    if (id == null) return NotFound();

    var material = await _context.Materials.FindAsync(id);
    if (material == null) return NotFound();

    var viewModel = new MaterialViewModel
    {
      MaterialId = material.MaterialId,
      Name = material.Name,
      Unit = material.Unit,
      Stock = material.Stock,
      UnitCost = material.UnitCost
    };

    return View(viewModel);
  }

  // POST: Materials/Edit/5
  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Edit(int id, MaterialViewModel viewModel)
  {
    if (id != viewModel.MaterialId) return NotFound();

    if (ModelState.IsValid)
    {
      try
      {
        var material = await _context.Materials.FindAsync(id);
        if (material == null) return NotFound();

        // Actualizar campos
        material.Name = viewModel.Name;
        material.Unit = viewModel.Unit;
        material.Stock = viewModel.Stock;
        material.UnitCost = viewModel.UnitCost;
        material.UpdatedAt = DateTime.Now;

        _context.Update(material);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Material actualizado correctamente.";
      }
      catch (DbUpdateConcurrencyException)
      {
        if (!MaterialExists(viewModel.MaterialId)) return NotFound();
        else throw;
      }

      return RedirectToAction(nameof(Index));
    }

    return View(viewModel);
  }

  // GET: Materials/Delete/5
  public async Task<IActionResult> Delete(int? id)
  {
    if (id == null) return NotFound();

    var material = await _context.Materials
      .FirstOrDefaultAsync(m => m.MaterialId == id);
    if (material == null) return NotFound();

    return View(material);
  }

  // POST: Materials/Delete/5
  [HttpPost]
  [ActionName("Delete")]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> DeleteConfirmed(int id)
  {
    var material = await _context.Materials.FindAsync(id);
    if (material != null)
    {
      _context.Materials.Remove(material);
      await _context.SaveChangesAsync();
      TempData["SuccessMessage"] = "Material eliminado correctamente.";
    }

    return RedirectToAction(nameof(Index));
  }

  private bool MaterialExists(int id)
  {
    return _context.Materials.Any(e => e.MaterialId == id);
  }
}