// Enums for Product Colour and Size
export enum ProductColour {
    Black = 'Black',
    White = 'White',
    Green = 'Green',
    Blue = 'Blue',
    Brown = 'Brown',
    Yellow = 'Yellow',
    Red = 'Red',
  }
  
  // Enum for Product Size using descriptive string values
  export enum ProductSize {
    Small = 'Small',
    Medium = 'Medium',
    Large = 'Large',
    XLarge = 'XLarge',
  }

// Function to map numeric values to actual color names
// Function to map numeric values to actual color names
export function mapProductColour(value: string): string {
    if (!value) {
      return 'Unknown Colour';
    }
    
    switch (value) { // No need for .toString() since value is already a string
        case ProductColour.Black:
            return 'Black';
        case ProductColour.White:
            return 'White';
        case ProductColour.Green:
            return 'Green';
        case ProductColour.Blue:
            return 'Blue';
        case ProductColour.Brown:
            return 'Brown';
        case ProductColour.Yellow:
            return 'Yellow';
        case ProductColour.Red:
            return 'Red';
        default:
            return 'Unknown Colour';
    }
  }
  
  // Function to map numeric values to actual size names
  export function mapProductSize(value: string): string {
    if (!value) {
      return 'Unknown Size';
    }
  
    switch (value) { // No need for .toString() since value is already a string
        case ProductSize.Small:
            return 'Small';
        case ProductSize.Medium:
            return 'Medium';
        case ProductSize.Large:
            return 'Large';
        case ProductSize.XLarge:
            return 'XLarge';
        default:
            return 'Unknown Size';
    }
  }
  