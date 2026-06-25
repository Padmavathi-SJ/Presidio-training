import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CropCycleComponent } from './crop-cycles.component';

describe('CropCycleComponent', () => {
  let component: CropCycleComponent;
  let fixture: ComponentFixture<CropCycleComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CropCycleComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(CropCycleComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
