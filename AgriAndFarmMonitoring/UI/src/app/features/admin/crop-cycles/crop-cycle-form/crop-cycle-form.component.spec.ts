import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CropCycleFormComponent } from './crop-cycle-form.component';

describe('CropCycleFormComponent', () => {
  let component: CropCycleFormComponent;
  let fixture: ComponentFixture<CropCycleFormComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CropCycleFormComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(CropCycleFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
