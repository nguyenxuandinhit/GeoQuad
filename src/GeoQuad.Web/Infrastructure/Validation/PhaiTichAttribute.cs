using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace GeoQuad.Web.Infrastructure.Validation;

/// <summary>
/// Bắt buộc một ô tích phải được tích (giá trị <c>true</c>).
///
/// Không dùng <c>[Range(typeof(bool), "true", "true")]</c> cho việc này: bộ chuyển đổi
/// unobtrusive sinh ra <c>data-val-range-min="True"</c>/<c>data-val-range-max="True"</c>,
/// còn luật <c>range</c> của jQuery Validate so sánh theo chuỗi — ô đã tích có giá trị
/// <c>"true"</c> nên <c>"true" &lt;= "True"</c> là sai và trang **luôn** báo chưa tích.
///
/// Thuộc tính này sinh <c>data-val-phaitich</c>, khớp với luật cùng tên khai báo trong
/// <c>_ValidationScriptsPartial.cshtml</c>, nên kiểm ở trình duyệt và ở máy chủ giống nhau.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class PhaiTichAttribute : ValidationAttribute, IClientModelValidator
{
    public override bool IsValid(object? value) => value is true;

    public void AddValidation(ClientModelValidationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var thongBao = FormatErrorMessage(context.ModelMetadata.GetDisplayName());
        context.Attributes.TryAdd("data-val", "true");
        context.Attributes.TryAdd("data-val-phaitich", thongBao);
    }
}
