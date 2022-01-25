
mergeInto(LibraryManager.library, {

copyToCanvas: function(ptr, w, h) 
{
      let data = Module.HEAPU8.slice(ptr, ptr + w * h * 4);
      let context = Module['canvas'].getContext('2d');
      let imageData = context.getImageData(0, 0, w, h);
      imageData.data.set(data);
      context.putImageData(imageData, 0, 0);
},

});
