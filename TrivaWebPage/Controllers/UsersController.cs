using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TrivaWebPage.Abstractions;
using TrivaWebPage.Models;
using TrivaWebPage.ViewModels.Admin;

namespace TrivaWebPage.Controllers;

public class UsersController : Controller
{
    private readonly IUser _repository;
    private readonly IPasswordHasher<User> _passwordHasher;

    public UsersController(IUser repository, IPasswordHasher<User> passwordHasher)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Kullanıcılar";
        var list = await _repository.GetAllAsync(cancellationToken);
        return View(list);
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Kullanıcılar";
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        return entity is null ? NotFound() : View(entity);
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.DisplayName = "Kullanıcılar";
        ViewBag.FormAction = "Create";
        return View("Form", new UserEditViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserEditViewModel model, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Kullanıcılar";
        ViewBag.FormAction = "Create";
        if (string.IsNullOrWhiteSpace(model.Password))
        {
            ModelState.AddModelError(nameof(model.Password), "Şifre gereklidir.");
        }

        if (!ModelState.IsValid)
        {
            return View("Form", model);
        }

        var existing = await _repository.GetByUserNameAsync(model.UserName.Trim(), cancellationToken);
        if (existing is not null)
        {
            ModelState.AddModelError(nameof(model.UserName), "Bu kullanıcı adı zaten kullanılıyor.");
            return View("Form", model);
        }

        var entity = new User
        {
            UserName = model.UserName.Trim(),
            PasswordHash = ""
        };
        entity.PasswordHash = _passwordHasher.HashPassword(entity, model.Password!);
        await _repository.CreateAsync(entity, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return NotFound();
        }

        ViewBag.DisplayName = "Kullanıcılar";
        ViewBag.FormAction = "Edit";
        return View("Form", new UserEditViewModel
        {
            Id = entity.Id,
            UserName = entity.UserName,
            Password = null
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UserEditViewModel model, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Kullanıcılar";
        ViewBag.FormAction = "Edit";
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View("Form", model);
        }

        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return NotFound();
        }

        var trimmed = model.UserName.Trim();
        if (!string.Equals(entity.UserName, trimmed, StringComparison.Ordinal))
        {
            var taken = await _repository.GetByUserNameAsync(trimmed, cancellationToken);
            if (taken is not null)
            {
                ModelState.AddModelError(nameof(model.UserName), "Bu kullanıcı adı zaten kullanılıyor.");
                return View("Form", model);
            }
        }

        entity.UserName = trimmed;
        if (!string.IsNullOrWhiteSpace(model.Password))
        {
            entity.PasswordHash = _passwordHasher.HashPassword(entity, model.Password);
        }

        await _repository.UpdateAsync(entity, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Kullanıcılar";
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        return entity is null ? NotFound() : View(entity);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken)
    {
        await _repository.DeleteAsync(id, cancellationToken);
        return RedirectToAction(nameof(Index));
    }
}
