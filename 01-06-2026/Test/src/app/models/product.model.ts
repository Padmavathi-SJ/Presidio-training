export class ProductModel{
    constructor(public title: string = "", 
        public description: string = "", 
        public price: number = 0,
        public imageUrl: string = ""){
            this.title = title;
            this.description = description;
            this.price = price;
            this.imageUrl = imageUrl;
        }
}