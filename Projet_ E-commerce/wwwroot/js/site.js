// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function addToCart(productId) {
    // Create a form dynamically
    var form = document.createElement('form');
    form.method = 'post';
    form.action = '/Cart/AddToCart';

    // Add hidden inputs
    var idInput = document.createElement('input');
    idInput.type = 'hidden';
    idInput.name = 'productId';
    idInput.value = productId;
    form.appendChild(idInput);

    var qtyInput = document.createElement('input');
    qtyInput.type = 'hidden';
    qtyInput.name = 'quantity';
    qtyInput.value = 1;
    form.appendChild(qtyInput);

    // Default size/color to empty for quick add (Controller falls back to default variant logic or stores empty)
    var sizeInput = document.createElement('input');
    sizeInput.type = 'hidden';
    sizeInput.name = 'size';
    sizeInput.value = '';
    form.appendChild(sizeInput);

    var colorInput = document.createElement('input');
    colorInput.type = 'hidden';
    colorInput.name = 'color';
    colorInput.value = '';
    form.appendChild(colorInput);

    // Append to body and submit
    document.body.appendChild(form);
    form.submit();
}

function toggleWishlist(productId) {
    // Placeholder for wishlist functionality
    console.log("Toggle wishlist for product: " + productId);
    // You could implement an AJAX call here later
}
