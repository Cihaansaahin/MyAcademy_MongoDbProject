using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Travel.Web.Services.CommentServices;

namespace Travel.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")] // Sadece rolü 'Admin' olan oturumlar girebilir
    public class CommentController : Controller
    {
        private readonly ICommentService _commentService;

        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        public async Task<IActionResult> Index()
        {
            var values = await _commentService.GetAllAsync();
            return View(values);
        }

        public async Task<IActionResult> Approve(string id)
        {
            await _commentService.ApproveCommentAsync(id);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(string id)
        {
            await _commentService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}