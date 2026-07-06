import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ImageCompressorService {
  /**
   * Compresses an image file to a maximum width/height while maintaining aspect ratio,
   * and reduces the JPEG quality to reduce file size.
   * @param file The original image file
   * @param maxWidth Maximum width in pixels
   * @param maxHeight Maximum height in pixels
   * @param quality JPEG quality (0.0 to 1.0)
   * @returns A Promise that resolves to the compressed File
   */
  compressImage(file: File, maxWidth = 1024, maxHeight = 1024, quality = 0.7): Promise<File> {
    return new Promise((resolve, reject) => {
      // If it's not an image, resolve with the original file
      if (!file.type.match(/image.*/)) {
        resolve(file);
        return;
      }

      const reader = new FileReader();
      reader.onerror = error => reject(error);
      reader.onload = (event: any) => {
        const img = new Image();
        img.onerror = error => reject(error);
        img.onload = () => {
          let width = img.width;
          let height = img.height;

          // Calculate new dimensions while keeping aspect ratio
          if (width > height) {
            if (width > maxWidth) {
              height = Math.round((height * maxWidth) / width);
              width = maxWidth;
            }
          } else {
            if (height > maxHeight) {
              width = Math.round((width * maxHeight) / height);
              height = maxHeight;
            }
          }

          const canvas = document.createElement('canvas');
          canvas.width = width;
          canvas.height = height;

          const ctx = canvas.getContext('2d');
          if (!ctx) {
            resolve(file);
            return;
          }
          ctx.drawImage(img, 0, 0, width, height);

          // Always output as JPEG for compression, or WebP if preferred
          canvas.toBlob(
            (blob) => {
              if (blob) {
                // Create a new File object with the blob
                const compressedFile = new File([blob], file.name, {
                  type: 'image/jpeg',
                  lastModified: Date.now()
                });
                resolve(compressedFile);
              } else {
                resolve(file);
              }
            },
            'image/jpeg',
            quality
          );
        };
        img.src = event.target.result;
      };
      reader.readAsDataURL(file);
    });
  }
}
