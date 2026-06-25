import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CropCycles } from './crop-cycles';

describe('CropCycles', () => {
  let component: CropCycles;
  let fixture: ComponentFixture<CropCycles>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CropCycles],
    }).compileComponents();

    fixture = TestBed.createComponent(CropCycles);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
